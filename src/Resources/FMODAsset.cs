using Godot;

namespace DefluoLib;

[Tool]
[GlobalClass]
public abstract partial class FMODAsset : Resource
{
    public Resource Asset;

    /// <summary>
    /// Path of the asset within the FMOD file system
    /// </summary>
    public string Path
    {
        get { return (string)Asset.Get("path"); }
    }

    /// <summary>
    /// Generated GUID of the asset
    /// </summary>
    public string GUID
    {
        get { return (string)Asset.Get("guid"); }
    }

    /// <summary>
    /// Name of the asset as defined in FMOD Studio
    /// </summary>
    public string Name
    {
        get { return (string)Asset.Get("name"); }
    }
}
