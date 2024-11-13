namespace DefluoLib;

using Godot;
using FMOD.Studio;
using System;

/// <summary>
/// A <see href="https://www.fmod.com/docs/2.03/studio/mixing.html#group-buses-and-routing">Bus</see> resource.
/// Can be exported from a node to be selected in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODBus : FMODResource
{
    private Bus bus;

    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-bus.html">Bus</see> object.
    /// </summary>
    public Bus Bus
    {
        get
        {
            if (bus.handle == IntPtr.Zero)
            {
                if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getBus(Path, out bus)))
                    throw new ArgumentException($"Invalid bus path {Path}");
            }
            return bus;
        }
    }

    public FMODBus(string path)
        : base(path) { }

    public FMODBus()
        : base() { }

    /// <summary>
    /// Pause state of bus. Can be used to pause or unpause all events routed to bus.
    /// </summary>
    public bool Paused
    {
        get
        {
            FMODCaller.CheckResult(Bus.getPaused(out var paused));
            return paused;
        }
        set => FMODCaller.CheckResult(Bus.setPaused(value));
    }

    /// <summary>
    /// Mute state of bus. Can be used to mute or unmute all events routed to bus.
    /// </summary>
    public bool Muted
    {
        get
        {
            FMODCaller.CheckResult(Bus.getMute(out var muted));
            return muted;
        }
        set => FMODCaller.CheckResult(Bus.setMute(value));
    }

    /// <summary>
    /// Volume of bus.
    /// </summary>
    public float Volume
    {
        get
        {
            FMODCaller.CheckResult(Bus.getVolume(out var volume));
            return volume;
        }
        set => FMODCaller.CheckResult(Bus.setVolume(value));
    }

    /// <summary>
    /// Stops all events routed to bus.
    /// </summary>
    /// <param name="allowFadeout">If true, events are allowed to fade out.</param>
    public void StopEvents(bool allowFadeout)
    {
        FMODCaller.CheckResult(
            Bus.stopAllEvents(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE)
        );
    }
}
