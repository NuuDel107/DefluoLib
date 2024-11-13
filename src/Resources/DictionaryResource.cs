namespace DefluoLib;

using Godot;

/// <summary>
/// A resource containing a dictionary.
/// Useful for saving and loading arbitrary data to and from the disk.
/// </summary>
public partial class DictionaryResource : Resource
{
    [Export]
    public Godot.Collections.Dictionary<string, Variant> Dictionary;

    public DictionaryResource()
    {
        Dictionary = [];
    }

}
