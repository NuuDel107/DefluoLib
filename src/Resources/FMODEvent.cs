using Godot;
using FMOD.Studio;
using System.Collections.Generic;

namespace DefluoLib;

[Tool]
[GlobalClass]
public partial class FMODEvent : FMODResource
{
    public EventDescription Description;

    public FMODEvent(string path)
        : base(path) { }

    public FMODEvent()
        : base() { }

    protected override void Init()
    {
        if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getEvent(Path, out Description)))
            throw new System.ArgumentException($"Invalid event path {Path}");
    }

    public bool Is3D
    {
        get
        {
            FMODCaller.CheckResult(Description.is3D(out var value));
            return value;
        }
    }
    public bool IsOneshot
    {
        get
        {
            FMODCaller.CheckResult(Description.isOneshot(out var value));
            return value;
        }
    }
    public bool IsSnapshot
    {
        get
        {
            FMODCaller.CheckResult(Description.isSnapshot(out var value));
            return value;
        }
    }
    public bool DopplerEnabled
    {
        get
        {
            FMODCaller.CheckResult(Description.isDopplerEnabled(out var value));
            return value;
        }
    }
    public float MinAttenuationDistance
    {
        get
        {
            FMODCaller.CheckResult(Description.getMinMaxDistance(out var value, out var _));
            return value;
        }
    }
    public float MaxAttenuationDistance
    {
        get
        {
            FMODCaller.CheckResult(Description.getMinMaxDistance(out var _, out var value));
            return value;
        }
    }
    public float SoundSize
    {
        get
        {
            FMODCaller.CheckResult(Description.getSoundSize(out var value));
            return value;
        }
    }

    private List<FMODEventInstance> OneshotInstances = new();

    public FMODEventInstance CreateInstance() => new(this);

    public void Play(params (string, Variant)[] parameters)
    {
        // Oneshot instances need to be added to a list
        // so they aren't garbage collected before they are released
        var instance = CreateInstance();
        OneshotInstances.Add(instance);

        instance.SetParameters(parameters);
        instance.Start();
        instance.Release();

        instance.Destroyed += () => OneshotInstances.Remove(instance);
    }

    public void PlayAttached(Node3D nodeToAttach, params (string, Variant)[] parameters)
    {
        var instance = CreateInstance();
        OneshotInstances.Add(instance);

        instance.AttachNode(nodeToAttach);
        instance.SetParameters(parameters);
        instance.Start();
        instance.Release();

        instance.Destroyed += () => OneshotInstances.Remove(instance);
    }
}
