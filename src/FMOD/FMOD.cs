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
    public FMOD.Studio.System StudioSystem;
    public Node3D Listener;
    private Vector3? listenerLastPosition = null;

    public void UpdateStudioSystem() => FMODCaller.CheckResult(StudioSystem.update());

    public event Action StudioSystemInitialized;
    public bool IsStudioSystemInitialized = false;

    public event Action StudioSystemUpdating;
    public event Action StudioSystemUpdated;
    public event Action LiveUpdateConnected;
    public event Action LiveUpdateDisconnected;

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

    public override void _Ready() { }

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

    public void Shutdown()
    {
        FMODCaller.CheckResult(StudioSystem.release());
    }

    public override void _ExitTree() => Shutdown();
}
