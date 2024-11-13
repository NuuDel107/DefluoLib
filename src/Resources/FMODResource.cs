namespace DefluoLib;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class FMODResource : Resource
{
    /// <summary>
    /// The local path of an FMOD resource.
    /// </summary>
    [Export]
    public string Path { get; set; }

    public FMODResource(string path)
    {
        Path = path;
    }

    public FMODResource() { }
}
