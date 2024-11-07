using Godot;
using FMOD.Studio;

namespace DefluoLib;

[Tool]
[GlobalClass]
public partial class FMODVCA : FMODResource
{
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
