using Godot;
using FMOD.Studio;

namespace DefluoLib;

[Tool]
[GlobalClass]
public partial class FMODBus : FMODResource
{
    public Bus Bus;

    public FMODBus(string path)
        : base(path) { }

    public FMODBus()
        : base() { }

    protected override void Init()
    {
        if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getBus(Path, out Bus)))
            throw new System.ArgumentException($"Invalid bus path {Path}");
    }

    public bool Paused
    {
        get
        {
            FMODCaller.CheckResult(Bus.getPaused(out var paused));
            return paused;
        }
        set => FMODCaller.CheckResult(Bus.setPaused(value));
    }

    public bool Muted
    {
        get
        {
            FMODCaller.CheckResult(Bus.getMute(out var muted));
            return muted;
        }
        set => FMODCaller.CheckResult(Bus.setMute(value));
    }

    public float Volume
    {
        get
        {
            FMODCaller.CheckResult(Bus.getVolume(out var volume));
            return volume;
        }
        set => FMODCaller.CheckResult(Bus.setVolume(value));
    }

    public void StopEvents(bool allowFadeout)
    {
        FMODCaller.CheckResult(
            Bus.stopAllEvents(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE)
        );
    }
}
