using Godot;
using FMOD;
using System;
using FMOD.Studio;
using System.Collections.Generic;

namespace DefluoLib;

/// <summary>
/// High-level manager for the FMOD API
/// </summary>
public partial class FMODHandler : Node
{
    /// <summary>
    /// Main <see href="https://www.fmod.com/docs/2.03/api/studio-api-system.html">Studio System</see> that is initialized at startup.
    /// </summary>

    public FMOD.Studio.System StudioSystem;
    public Node3D Listener;
    private Vector3? listenerLastPosition = null;

    /// <summary>
    /// Updates the studio system. <see href="https://www.fmod.com/docs/2.03/api/studio-guide.html#studio-system-processing">See here for more info</see>
    /// </summary>
    public void UpdateStudioSystem() => FMODCaller.CheckResult(StudioSystem.update());

    /// <summary>
    /// Fired once at startup when studio system finishes initializing.
    /// </summary>
    public event Action StudioSystemInitialized;

    /// <summary>
    /// Whether or not studio system is initialized yet.
    /// </summary>
    public bool IsStudioSystemInitialized = false;

    /// <summary>
    /// Fired when studio system starts updating
    /// </summary>
    public event Action StudioSystemUpdating;

    /// <summary>
    /// Fired when studio system has finished updating
    /// </summary>
    public event Action StudioSystemUpdated;

    /// <summary>
    /// Fired when connected to an FMOD Studio client for <see href="https://www.fmod.com/docs/2.03/studio/editing-during-live-update.html">live updating</see>
    /// </summary>
    public event Action LiveUpdateConnected;

    /// <summary>
    /// Fired when disconnected from an FMOD Studio client
    /// </summary>
    public event Action LiveUpdateDisconnected;

    /// <summary>
    /// List of banks currently loaded in the studio system
    /// </summary>
    /// <returns></returns>
    public List<FMODBank> LoadedBanks = new();

    private FMOD.Studio.SYSTEM_CALLBACK studioSystemCallback;

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint())
            return;

        Name = "FMOD";
        ProcessMode = ProcessModeEnum.Always;

        FMOD.Studio.System.create(out StudioSystem);
        StudioSystem.initialize(
            512,
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            (IntPtr)0
        );

        studioSystemCallback = (_, type, _, _) =>
        {
            switch (type)
            {
                case FMOD.Studio.SYSTEM_CALLBACK_TYPE.PREUPDATE:
                    StudioSystemUpdating?.Invoke();
                    break;
                case FMOD.Studio.SYSTEM_CALLBACK_TYPE.POSTUPDATE:
                    StudioSystemUpdated?.Invoke();
                    break;
                case FMOD.Studio.SYSTEM_CALLBACK_TYPE.LIVEUPDATE_CONNECTED:
                    LiveUpdateConnected?.Invoke();
                    break;
                case FMOD.Studio.SYSTEM_CALLBACK_TYPE.LIVEUPDATE_DISCONNECTED:
                    LiveUpdateDisconnected?.Invoke();
                    break;
            }
            return RESULT.OK;
        };
        StudioSystem.setCallback(studioSystemCallback);

        var startupBanks = (Godot.Collections.Array<string>)
            ProjectSettings.GetSetting("DefluoLib/FMOD/BanksToLoadOnStartup");

        foreach (var bankPath in startupBanks)
        {
            var bank = new FMODBank(bankPath);
            bank.Load();
            LoadedBanks.Add(bank);
        }

        UpdateStudioSystem();
        StudioSystemUpdated += OnInitialization;
    }

    private void OnInitialization()
    {
        IsStudioSystemInitialized = true;
        StudioSystemInitialized?.Invoke();
        StudioSystemUpdated -= OnInitialization;
    }

    /// <summary>
    /// Sets the <c>Node3D</c> to be used as a listener.
    /// All spatialized events are played based on their position in relation to the listener.
    /// <see href="https://www.fmod.com/docs/2.03/api/white-papers-studio-3d-events.html">See here for more info</see>
    /// </summary>
    /// <param name="listener"></param>
    public void AttachListener(Node3D listener)
    {
        Listener = listener;
    }

    private Dictionary<FMODEventInstance, Vector3?> attachedEventInstances = new();

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        if (Listener == null)
            return;
        foreach (var (eventInstance, lastPosition) in attachedEventInstances)
        {
            if (eventInstance.IsReleased)
            {
                attachedEventInstances.Remove(eventInstance);
                continue;
            }
            var attr = Calculate3DAttributes(
                delta,
                eventInstance.AttachedNode.GlobalPosition,
                lastPosition
            );
            eventInstance.Update3DAttributes(attr);
            attachedEventInstances[eventInstance] = eventInstance.AttachedNode.GlobalPosition;
        }

        var listenerAttributes = Calculate3DAttributes(
            delta,
            Listener.GlobalPosition,
            listenerLastPosition,
            -Listener.GlobalBasis.Z,
            Listener.GlobalBasis.Y
        );
        StudioSystem.setListenerAttributes(0, listenerAttributes);
        listenerLastPosition = Listener.GlobalPosition;

        UpdateStudioSystem();
    }

    private ATTRIBUTES_3D Calculate3DAttributes(
        double delta,
        Vector3 pos,
        Vector3? lastPos,
        Vector3 forward = new(),
        Vector3 up = new()
    )
    {
        var attributes = new ATTRIBUTES_3D();
        if (forward == Vector3.Zero)
            forward = Vector3.Forward;
        if (up == Vector3.Zero)
            up = Vector3.Up;

        attributes.position = pos.ToFMOD();
        if (lastPos == null)
            attributes.velocity = Vector3.Zero.ToFMOD();
        else
            attributes.velocity = ((pos - lastPos.Value) * (float)delta).ToFMOD();

        attributes.forward = forward.ToFMOD();
        attributes.up = up.ToFMOD();

        return attributes;
    }

    public void AddAttachedEventInstance(FMODEventInstance attachedEventInstance) =>
        attachedEventInstances.Add(attachedEventInstance, null);

    /// <summary>
    /// Shuts down the studio system.
    /// Called automatically when this manager node exits the SceneTree.
    /// </summary>
    public void Shutdown()
    {
        FMODCaller.CheckResult(StudioSystem.release());
    }

    public override void _ExitTree() => Shutdown();
}
