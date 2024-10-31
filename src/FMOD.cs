using Godot;
using System.Collections.Generic;
using System.Linq;

namespace DefluoLib;

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
    public void PlayEvent(string eventPath, params (string Name, Variant Value)[] parameters) =>
        CreateEventInstance(eventPath).PlayOneShot(parameters);

    /// <summary>
    /// Plays event once and releases the event instance afterwards
    /// </summary>
    /// <param name="eventAsset">Event asset that the played instance will be created from</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(FMODEvent eventAsset, params (string Name, Variant Value)[] parameters) =>
        PlayEvent(eventAsset.Path, parameters);

    /// <summary>
    /// Plays event once attached to a 3D node and releases the event instance afterwards
    /// </summary>
    /// <param name="eventPath">Name of the event</param>
    /// <param name="attachedNode">3D node that is attached to the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(
        string eventPath,
        Node3D attachedNode,
        params (string Name, Variant Value)[] parameters
    ) => CreateEventInstance(eventPath, attachedNode).PlayOneShot(parameters);

    /// <summary>
    /// Plays event once attached to a 3D node and releases the event instance afterwards
    /// </summary>
    /// <param name="eventAsset">Event asset that the played instance will be created from</param>
    /// <param name="attachedNode">3D node that is attached to the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void PlayEvent(
        FMODEvent eventAsset,
        Node3D attachedNode,
        params (string Name, Variant Value)[] parameters
    ) => PlayEvent(eventAsset.Path, attachedNode, parameters);

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
    /// Sets the value of a number parameter in global scope
    /// </summary>
    /// <param name="parameter">Global parameter asset</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetGlobalParameter(FMODParameter parameter, float value) =>
        GDScriptNode.Call("set_global_float_parameter", parameter.Name, value);

    /// <summary>
    /// Sets the value of a labeled parameter in global scope
    /// </summary>
    /// <param name="parameter">Global parameter asset</param>
    /// <param name="value">Value to set parameter to</param>
    public void SetGlobalParameter(FMODParameter parameter, string value) =>
        GDScriptNode.Call("set_global_label_parameter", parameter.Name, value);

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

    /// <summary>
    /// Shuts down the FMOD engine.
    /// If the FMOD addon's automatic shutdown has been disabled, this should be called manually when closing the game
    /// </summary>
    public void Shutdown()
    {
        GDScriptNode.Call("shutdown");
    }
}
