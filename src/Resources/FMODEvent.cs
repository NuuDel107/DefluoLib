using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// FMOD event asset that event instances can be created and played from.
/// Can also represent a snapshot.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODEvent : FMODAsset
{
    public Resource EventAsset
    {
        get { return Asset; }
        set { Asset = value; }
    }

    public bool Is3D
    {
        get { return (bool)Asset.Get("is_3d"); }
    }
    public bool IsOneshot
    {
        get { return (bool)Asset.Get("is_oneshot"); }
    }
    public bool IsSnapshot
    {
        get { return (bool)Asset.Get("is_snapshot"); }
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "EventAsset" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "EventAsset" }
            }
        };
        return properties;
    }

    /// <summary>
    /// Plays event once and releases the event instance afterwards
    /// </summary>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void Play(params (string Name, Variant Value)[] parameters) =>
        Defluo.FMOD.PlayEvent(this, parameters);

    /// <summary>
    /// Plays event once attached to a 3D node and releases the event instance afterwards
    /// </summary>
    /// <param name="attachedNode">3D node that is attached to the event</param>
    /// <param name="parameters">Event parameters that will be set before playing event instance</param>
    public void Play(Node3D attachedNode, params (string Name, Variant Value)[] parameters) =>
        Defluo.FMOD.PlayEvent(this, attachedNode, parameters);

    /// <summary>
    /// Creates a new event instance
    /// </summary>
    public FMODEventInstance CreateInstance() => Defluo.FMOD.CreateEventInstance(this);

    /// <summary>
    /// Creates a new event instance that uses positional data from attached node to spatialize the audio
    /// </summary>
    /// <param name="nodeToAttach">3D node to attach to instance</param>
    public FMODEventInstance CreateInstance(Node3D nodeToAttach) =>
        Defluo.FMOD.CreateEventInstance(this, nodeToAttach);
}
