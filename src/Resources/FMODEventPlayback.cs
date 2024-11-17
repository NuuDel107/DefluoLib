namespace DefluoLib;

using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class FMODEventPlayback : Resource
{
    [Export]
    public FMODEvent Event { get; set; }

    [Export]
    public Dictionary<string, Variant> Arguments = new();

    public void Play()
    {
        if (Event == null)
            return;

        if (Engine.IsEditorHint())
        {
            var args = Arguments.Select(x => (x.Key, x.Value)).ToArray();
            DefluoLib.FMODLister.PlayEvent(Event.Path, args);
        }
        else
        {
            Event.Play(Arguments.Select(x => (x.Key, x.Value)).ToArray());
        }
    }
}
