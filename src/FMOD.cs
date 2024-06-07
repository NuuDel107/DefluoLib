using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DefluoLib;

/// <summary>
/// Base class for an FMOD instance
/// </summary>
public class FMODInstance
{
    /// <summary>
    /// Calls specified function in the GDScript node
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    protected virtual Variant Call(string method, params Variant[] args) =>
        Defluo.FMOD.GDScriptNode.Call(method, args);
}

/// <summary>
/// Instance of an FMOD bus
/// </summary>
public class FMODBusInstance : FMODInstance
{
    private Variant Bus;

    internal FMODBusInstance(string busPath)
    {
        Bus = Defluo.FMOD.GDScriptNode.Call("get_bus", busPath);
    }

    /// <summary>
    /// Sets volume of bus
    /// </summary>
    public void SetVolume(float volume) => Call("set_bus_volume", Bus, volume);

    /// <summary>
    /// Pauses or unpauses bus
    /// </summary>
    public void SetPaused(bool paused) => Call("set_bus_paused", Bus, paused);
}

/// <summary>
/// Instance of an FMOD VCA
/// </summary>
public class FMODVCAInstance : FMODInstance
{
    private Variant VCA;

    internal FMODVCAInstance(string vcaPath)
    {
        VCA = Defluo.FMOD.GDScriptNode.Call("get_vca", vcaPath);
    }

    /// <summary>
    /// Sets volume of VCA
    /// </summary>
    public void SetVolume(float volume) => Call("set_vca_volume", VCA, volume);
}

/// <summary>
/// Instance of an FMOD bank
/// </summary>
public class FMODBankInstance : FMODInstance
{
    private Variant Bank;

    private string FilePath;

    public bool IsLoaded;

    internal FMODBankInstance(string bankFilePath)
    {
        FilePath = bankFilePath;
    }

    public void Load()
    {
        Call("load_bank", FilePath);
        Bank = Call("get_bank", FilePath);
        IsLoaded = true;
    }

    public void Unload()
    {
        Call("unload_bank", Bank);
        IsLoaded = false;
    }
}

public enum FMODEventCallbackType : uint
{
    Created = 0x00000001,
    Destroyed = 0x00000002,
    Starting = 0x00000004,
    Started = 0x00000008,
    Restarted = 0x00000010,
    Stopped = 0x00000020,
    StartFailed = 0x00000040,
    CreateProgrammerSound = 0x00000080,
    DestroyProgrammerSound = 0x00000100,
    PluginCreated = 0x00000200,
    PluginDestroyed = 0x00000400,
    TimelineMarker = 0x00000800,
    TimelineBeat = 0x00001000,
    SoundPlayed = 0x00002000,
    SoundStopped = 0x00004000,
    RealToVirtual = 0x00008000,
    VirtualToReal = 0x00010000,
    StartEventCommand = 0x00020000,
    NestedTimelineBeat = 0x00040000,
    All = 0xFFFFFFFF,
}

/// <summary>
/// Instance of an FMOD event
/// </summary>
public class FMODEventInstance : FMODInstance
{
    /// <summary>
    /// If <c>true</c>, event instance has been released from memory
    /// meaning it can't be used anymore
    /// </summary>
    public bool Released = false;

    public Node3D AttachedNode;

    private Variant Instance;

    internal FMODEventInstance(string eventPath, Node3D nodeToAttach = null)
    {
        Instance = Call("create_event_instance", eventPath);
        Attach(nodeToAttach);

        // Set state to released when the event instance is freed from memory
        AddCallback(() => Released = true, FMODEventCallbackType.Destroyed);
    }

    protected override Variant Call(string method, params Variant[] args)
    {
        if (Released)
            GD.PushWarning($"Tried to call function on released instance of event");
        return base.Call(method, args);
    }

    /// <summary>
    /// Plays instance once and releases it afterwards
    /// </summary>
    public void PlayOneShot()
    {
        Start();
        Release();
    }

    /// <summary>
    /// Plays instance once with specified parameters and releases it afterwards
    /// </summary>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayOneShot(params (string Name, Variant Value)[] parameters)
    {
        foreach (var parameter in parameters)
            SetParameter(parameter);

        PlayOneShot();
    }

    /// <summary>
    /// Sets the value of a number parameter
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetParameter(string paramName, float value) =>
        Call("set_instance_float_parameter", Instance, paramName, value);

    /// <summary>
    /// Sets the value of a labeled parameter
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetParameter(string paramName, string value) =>
        Call("set_instance_label_parameter", Instance, paramName, value);

    /// <summary>
    /// Sets the value of a parameter
    /// </summary>
    /// <param name="parameter">Tuple containing the name of the parameter and value to set parameter to</param>
    public void SetParameter((string Name, Variant Value) parameter)
    {
        if (parameter.Value.VariantType == Variant.Type.String)
            Call("set_instance_label_parameter", Instance, parameter.Name, parameter.Value);
        else
            Call("set_instance_float_parameter", Instance, parameter.Name, parameter.Value);
    }

    /// <summary>
    /// Starts playback of the event
    /// </summary>
    public void Start() => Call("start_instance", Instance);

    /// <summary>
    /// Stops the event playback
    /// </summary>
    /// <param name="allowFadeout">If false, event stops immediately</param>
    public void Stop(bool allowFadeout = true) => Call("stop_instance", Instance, allowFadeout);

    /// <summary>
    /// Pauses or unpauses the event playback
    /// </summary>
    public void SetPaused(bool paused) => Call("pause_instance", Instance, paused);

    /// <summary>
    /// Queues event instance for release. The released state is updated when the instance is released.
    /// </summary>
    public void Release() => Call("release_instance", Instance);

    /// <summary>
    /// Attaches a Node3D to the instance, making the audio spatialized in the 3D space
    /// </summary>
    /// <param name="node"></param>
    public void Attach(Node3D node)
    {
        if (node != null)
            AttachedNode = node;
    }

    /// <summary>
    /// Updates the 3D attributes of event instance if a node has been attached to it.
    /// </summary>
    internal void Update3D()
    {
        if (AttachedNode != null)
            Call("update_instance_3d", Instance, AttachedNode.GlobalPosition);
    }

    /// <summary>
    /// Add a callback for an event relating to the event instance
    /// </summary>
    /// <param name="callback">Action to be ran when callback is called</param>
    /// <param name="callbackType">Specifies the event that triggers the callback</param>
    public void AddCallback(Action callback, FMODEventCallbackType callbackType)
    {
        var callable = Callable.From<Variant>((_) => callback());
        Call("add_instance_callback", Instance, callable, (uint)callbackType);
    }
}

/// <summary>
/// Handler for interfacing with the FMOD Godot integration
/// </summary>
public partial class FMODHandler : Node
{
    internal static readonly string GDScriptPath = "res://addons/DefluoLib/src/FMOD.gd";
    internal Node GDScriptNode;

    private List<FMODEventInstance> createdEventInstances = new();

    public override void _EnterTree()
    {
        Name = "FMOD";
    }

    public override void _Ready()
    {
        var script = GD.Load<GDScript>(GDScriptPath);
        GDScriptNode = (Node)script.New();
        AddChild(GDScriptNode);
    }

    public override void _Process(double delta)
    {
        // Loop through created event instances and update their 3D attributes
        foreach (var instance in createdEventInstances.ToList())
        {
            if (instance.Released)
                createdEventInstances.Remove(instance);
            else
                instance.Update3D();
        }
    }

    public override void _ExitTree()
    {
        StopEventInstances(allowFadeout: false);
        createdEventInstances.Clear();
    }

    /// <summary>
    /// Plays event once and releases the event instance afterwards
    /// </summary>
    /// <param name="eventPath">Name of the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(string eventPath, params (string Name, Variant Value)[] parameters) => CreateEventInstance(eventPath).PlayOneShot(parameters);

    /// <summary>
    /// Plays event once and releases the event instance afterwards
    /// </summary>
    /// <param name="eventAsset">Event asset that the played instance will be created from</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(FMODEvent eventAsset, params (string Name, Variant Value)[] parameters) => PlayEvent(eventAsset.Path, parameters);

    /// <summary>
    /// Plays event once attached to a 3D node and releases the event instance afterwards
    /// </summary>
    /// <param name="eventPath">Name of the event</param>
    /// <param name="attachedNode">3D node that is attached to the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(string eventPath, Node3D attachedNode, params (string Name, Variant Value)[] parameters) =>
        CreateEventInstance(eventPath, attachedNode).PlayOneShot(parameters);

    /// <summary>
    /// Plays event once attached to a 3D node and releases the event instance afterwards
    /// </summary>
    /// <param name="eventAsset">Event asset that the played instance will be created from</param>
    /// <param name="attachedNode">3D node that is attached to the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(FMODEvent eventAsset, Node3D attachedNode, params (string Name, Variant Value)[] parameters) =>
        PlayEvent(eventAsset.Path, attachedNode, parameters);

    /// <summary>
    /// Sets the value of a number parameter in global scope
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetGlobalParameter(string paramName, float value) =>
        GDScriptNode.Call("set_global_float_parameter", paramName, value);

    /// <summary>
    /// Sets the value of a labeled parameter in global scope
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetGlobalParameter(string paramName, string value) =>
        GDScriptNode.Call("set_global_label_parameter", paramName, value);

    /// <summary>
    /// Sets the value of a parameter in global scope
    /// </summary>
    /// <param name="parameter">Tuple containing the name of the parameter and value to set parameter to</param>
    public void SetGlobalParameter((string Name, Variant Value) parameter)
    {
        if (parameter.Value.VariantType == Variant.Type.String)
            GDScriptNode.Call("set_global_label_parameter", parameter.Name, parameter.Value);
        else
            GDScriptNode.Call("set_global_float_parameter", parameter.Name, parameter.Value);
    }


    /// <summary>
    /// Stops all created event instances and releases them
    /// </summary>
    public void StopEventInstances(bool allowFadeout = true)
    {
        foreach (var instance in createdEventInstances)
        {
            instance.Stop(allowFadeout);
            instance.Release();
        }
    }

    /// <summary>
    /// Creates a new event instance
    /// </summary>
    /// <param name="eventPath">Path of the event to create an instance from</param>
    public FMODEventInstance CreateEventInstance(string eventPath)
    {
        FMODEventInstance instance = new(eventPath);
        createdEventInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Creates a new event instance that uses positional data from attached node to spatialize the audio
    /// </summary>
    /// <param name="eventPath">Path of the event to create an instance from</param>
    /// <param name="nodeToAttach">3D node to attach to instance</param>
    public FMODEventInstance CreateEventInstance(string eventPath, Node3D nodeToAttach)
    {
        FMODEventInstance instance = new(eventPath, nodeToAttach);
        createdEventInstances.Add(instance);
        return instance;
    }

    /// <summary>
    /// Creates a new event instance
    /// </summary>
    /// <param name="eventAsset">Event asset that the instance will be created from</param>
    public FMODEventInstance CreateEventInstance(FMODEvent eventAsset) =>
        CreateEventInstance(eventAsset.Path);

    /// <summary>
    /// Creates a new event instance that uses positional data from attached node to spatialize the audio
    /// </summary>
    /// <param name="eventAsset">Event asset that the instance will be created from</param>
    /// <param name="nodeToAttach">3D node to attach to instance</param>
    public FMODEventInstance CreateEventInstance(FMODEvent eventAsset, Node3D nodeToAttach) =>
        CreateEventInstance(eventAsset.Path, nodeToAttach);

    /// <summary>
    /// Returns an instance of a bus
    /// </summary>
    /// <param name="busPath">Path of the bus</param>
    public FMODBusInstance GetBus(string busPath) => new(busPath);

    /// <summary>
    /// Returns an instance of a bus
    /// </summary>
    /// <param name="busAsset">Asset that bus will be instantiated from</param>
    public FMODBusInstance GetBus(FMODBus busAsset) => new(busAsset.Path);

    /// <summary>
    /// Returns an instance of a VCA
    /// </summary>
    /// <param name="vcaPath">Path of the VCA</param>
    public FMODVCAInstance GetVCA(string vcaPath) => new(vcaPath);

    /// <summary>
    /// Returns an instance of a VCA
    /// </summary>
    /// <param name="vcaAsset">Asset that VCA will be instantiated from</param>
    public FMODVCAInstance GetVCA(FMODVCA vcaAsset) => new(vcaAsset.Path);

    /// <summary>
    /// Returns an instance of a bank
    /// </summary>
    /// <param name="bankFilePath">File path in the project that leads to the .bank file that should be loaded</param>
    public FMODBankInstance GetBank(string bankFilePath) => new(bankFilePath);

    /// <summary>
    /// Returns an instance of a bank
    /// </summary>
    /// <param name="bankFilePath">File path in the project that leads to the .bank file that should be loaded</param>
    public FMODBankInstance GetBank(FMODBank bankAsset) => new(bankAsset.FilePath);
}
