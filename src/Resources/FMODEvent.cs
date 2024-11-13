using Godot;
using FMOD.Studio;
using System.Collections.Generic;
using System;

namespace DefluoLib;

/// <summary>
/// An <see href="https://www.fmod.com/docs/2.03/studio/fmod-studio-concepts.html#event">Event</see> resource.
/// Can be exported from a node to be selected in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODEvent : FMODResource
{
    private EventDescription eventDescription;

    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-eventdescription.html">event description</see>.
    /// </summary>
    public EventDescription Description
    {
        get
        {
            if (eventDescription.handle == IntPtr.Zero)
            {
                if (
                    !FMODCaller.CheckResult(
                        Defluo.FMOD.StudioSystem.getEvent(Path, out eventDescription)
                    )
                )
                    throw new System.ArgumentException($"Invalid event path {Path}");
            }
            return eventDescription;
        }
    }

    public FMODEvent(string path)
        : base(path) { }

    public FMODEvent()
        : base() { }

    /// <summary>
    /// If event is <see href="https://www.fmod.com/docs/2.03/studio/glossary.html#spatialization">3D spatialized</see>
    /// </summary>
    public bool Is3D
    {
        get
        {
            FMODCaller.CheckResult(Description.is3D(out var value));
            return value;
        }
    }

    /// <summary>
    /// If event is "oneshot", a.k.a. it is guaranteed to terminate without intervention in bounded time after being started.
    /// You should only use the <c>Play</c> function for playback of oneshot events.
    /// </summary>
    /// <value></value>
    public bool IsOneshot
    {
        get
        {
            FMODCaller.CheckResult(Description.isOneshot(out var value));
            return value;
        }
    }

    /// <summary>
    /// If event is a <see href="https://www.fmod.com/docs/2.03/studio/glossary.html#snapshot">snapshot</see>
    /// </summary>
    public bool IsSnapshot
    {
        get
        {
            FMODCaller.CheckResult(Description.isSnapshot(out var value));
            return value;
        }
    }

    /// <summary>
    /// If simulation of the <see href="https://en.wikipedia.org/wiki/Doppler_effect">Doppler effect</see> is enabled for event
    /// </summary>
    /// <value></value>
    public bool DopplerEnabled
    {
        get
        {
            FMODCaller.CheckResult(Description.isDopplerEnabled(out var value));
            return value;
        }
    }

    /// <summary>
    /// The minimum distance for attenuation in the 3D spatializer
    /// </summary>
    /// <value></value>
    public float MinAttenuationDistance
    {
        get
        {
            FMODCaller.CheckResult(Description.getMinMaxDistance(out var value, out var _));
            return value;
        }
    }

    /// <summary>
    /// The maximum distance for attenuation in the 3D spatializer
    /// </summary>
    /// <value></value>
    public float MaxAttenuationDistance
    {
        get
        {
            FMODCaller.CheckResult(Description.getMinMaxDistance(out var _, out var value));
            return value;
        }
    }

    /// <summary>
    /// The sound size of the 3D spatializer
    /// </summary>
    /// <value></value>
    public float SoundSize
    {
        get
        {
            FMODCaller.CheckResult(Description.getSoundSize(out var value));
            return value;
        }
    }

    private List<FMODEventInstance> OneshotInstances = new();

    /// <summary>
    /// Creates a new <see href="https://www.fmod.com/docs/2.03/studio/fmod-studio-concepts.html#events-and-event-instances">event instance</see> from the event.
    /// </summary>
    public FMODEventInstance CreateInstance() => new(this);

    /// <summary>
    /// Starts the event and releases it when complete.
    /// <br/>
    /// Use this to play oneshot events that are stopped automatically.
    /// If the event loops, create an event instance manually.
    /// </summary>
    /// <param name="parameters">List of parameters are initialized to given values</param>
    public void Play(params (string name, Variant value)[] parameters)
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

    /// <summary>
    /// Starts the event attached to a 3D node and releases it when complete.
    /// <br/>
    /// Use this to play oneshot events that are stopped automatically.
    /// If the event loops, create an event instance manually.
    /// </summary>
    /// <param name="nodeToAttach">3D node that is used to update 3D attributes of event instance during playback</param>
    /// <param name="parameters">List of parameters are initialized to given values</param>
    public void PlayAttached(Node3D nodeToAttach, params (string name, Variant value)[] parameters)
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
