using Godot;
using System;
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

/// <summary>
/// Possible callbacks that can be subscribed to in an event instance
/// </summary>
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
        SetParameters(parameters);
        PlayOneShot();
    }

    /// <summary>
    /// Plays instance once with specified parameters and releases it afterwards
    /// </summary>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayOneShot(params (FMODParameter parameter, Variant Value)[] parameters) =>
        PlayOneShot(ParameterAssetsToStrings(parameters));

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
    /// Sets the value of a number parameter
    /// </summary>
    /// <param name="parameter">Parameter asset</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetParameter(FMODParameter parameter, float value) =>
        SetParameter(parameter.Name, value);

    /// <summary>
    /// Sets the value of a labeled parameter
    /// </summary>
    /// <param name="parameter">Parameter asset</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetParameter(FMODParameter parameter, string value) =>
        SetParameter(parameter.Name, value);

    /// <summary>
    /// Sets values of multiple parameters
    /// </summary>
    /// <param name="parameters">Tuples containing the name of the parameter and value to set parameter to</param>
    public void SetParameters(params (string Name, Variant Value)[] parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Value.VariantType == Variant.Type.String)
                SetParameter(parameter.Name, parameter.Value.As<string>());
            else
                SetParameter(parameter.Name, parameter.Value.As<string>());
        }
    }

    /// <summary>
    /// Sets values of multiple parameters
    /// </summary>
    /// <param name="parameters">Tuples containing the parameter asset and value to set it to</param>
    public void SetParameters(params (FMODParameter parameter, Variant Value)[] parameters) =>
        SetParameters(ParameterAssetsToStrings(parameters));

    private (string Name, Variant Value)[] ParameterAssetsToStrings(
        (FMODParameter parameter, Variant value)[] parameters
    )
    {
        var query = from param in parameters select (param.parameter.Name, param.value);
        return query.ToArray();
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
    public void SetPaused(bool paused) => Call("set_instance_paused", Instance, paused);

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
