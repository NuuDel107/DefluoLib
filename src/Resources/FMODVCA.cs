namespace DefluoLib;

using Godot;
using FMOD.Studio;
using System;

/// <summary>
/// A <see href="https://www.fmod.com/docs/2.03/studio/mixing.html#vcas">VCA</see> resource.
/// Can be exported from a node to be selected in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODVCA : FMODResource
{
    private VCA vca;

    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-vca.html">VCA</see> object.
    /// </summary>
    public VCA VCA
    {
        get
        {
            if (vca.handle == IntPtr.Zero)
            {
                if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getVCA(Path, out vca)))
                    throw new ArgumentException($"Invalid VCA path {Path}");
            }
            return vca;
        }
    }

    public FMODVCA(string path)
        : base(path) { }

    public FMODVCA()
        : base() { }

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
