using Godot;
using FMOD.Studio;

namespace DefluoLib;

/// <summary>
/// A <see href="https://www.fmod.com/docs/2.03/studio/mixing.html#vcas">VCA</see> resource.
/// Can be exported from a node to be selected in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODVCA : FMODResource
{
    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-vca.html">VCA</see> object.
    /// </summary>
    public VCA VCA;

    public FMODVCA(string path)
        : base(path) { }

    public FMODVCA()
        : base() { }

    protected override void Init()
    {
        if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getVCA(Path, out VCA)))
            throw new System.ArgumentException($"Invalid VCA path {Path}");
    }

    /// <summary>
    /// Volume of VCA.
    /// </summary>
    public float Volume
    {
        get
        {
            FMODCaller.CheckResult(VCA.getVolume(out var volume));
            return volume;
        }
        set => FMODCaller.CheckResult(VCA.setVolume(value));
    }
}
