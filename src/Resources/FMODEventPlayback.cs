namespace DefluoLib;

using System.Linq;
using Godot;
using Godot.Collections;

/// <summary>
/// A resource containing arguments for playing an event.
/// Can be used for various purposes.<br/><br/>
/// For example, exporting a function with this resource as an argument
/// allows for events to be played straight from an animation track:
/// <code>
/// public partial class Character : CharacterBody3D
/// {
///     // Calling this function from a call method track will play the event
///     public void PlaySound(FMODEventPlayback eventPlayback) => eventPlayback.Play(this);
/// }
/// </code>
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODEventPlayback : Resource
{
    [Export]
    public FMODEvent Event { get; set; }

    [Export]
    public bool PlayAttached;

    [Export]
    public Dictionary<string, Variant> Arguments = new();

    public void Play(Node3D? nodeToAttach = null)
    {
        if (Event == null)
            return;

        var args = Arguments.Select(x => (x.Key, x.Value)).ToArray();
        if (Engine.IsEditorHint())
        {
            DefluoLib.FMODLister.PlayEvent(Event.Path, args);
        }
        else
        {
            if (PlayAttached && nodeToAttach != null)
                Event.PlayAttached(nodeToAttach, args);
            else
                Event.Play(args);
        }
    }
}
