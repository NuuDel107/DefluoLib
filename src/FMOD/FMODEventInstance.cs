using Godot;
using FMOD;
using FMOD.Studio;
using System;

namespace DefluoLib;

/// <summary>
/// An <see href="https://www.fmod.com/docs/2.03/studio/fmod-studio-concepts.html#events-and-event-instances">instance</see> of an event.
/// </summary>
public class FMODEventInstance
{
    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-eventinstance.html">instance</see>
    /// </summary>
    public EventInstance Instance;

    /// <summary>
    /// The <see href="https://www.fmod.com/docs/2.03/api/studio-api-eventdescription.html">description</see> of the event instance
    /// </summary>
    public EventDescription Description;

    /// <summary>
    /// Node that the event instance uses to update 3D attributes.
    /// </summary>
    public Node3D AttachedNode;

    /// <summary>
    /// The <see href="https://www.fmod.com/docs/2.03/api/studio-api-common.html#fmod_studio_playback_state">playback state</see> of event instance
    /// </summary>
    /// <value></value>
    public PLAYBACK_STATE PlaybackState
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPlaybackState(out var state));
            return state;
        }
    }

    /// <summary>
    /// Pause state of event instance
    /// </summary>
    public bool Paused
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPaused(out var paused));
            return paused;
        }
        set => FMODCaller.CheckResult(Instance.setPaused(value));
    }

    /// <summary>
    /// Volume of event instance
    /// </summary>
    public float Volume
    {
        get
        {
            FMODCaller.CheckResult(Instance.getVolume(out var volume));
            return volume;
        }
        set => FMODCaller.CheckResult(Instance.setVolume(value));
    }

    /// <summary>
    /// Pitch of event instance
    /// </summary>
    public float Pitch
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPitch(out var pitch));
            return pitch;
        }
        set => FMODCaller.CheckResult(Instance.setPitch(value));
    }

    /// <summary>
    /// If event instance is playing
    /// </summary>
    public bool IsPlaying => PlaybackState == PLAYBACK_STATE.PLAYING;

    /// <summary>
    /// If event instance has been released from memory
    /// </summary>
    /// <value></value>
    public bool IsReleased { get; set; } = false;

    /// <summary>
    /// Called when an instance is fully created.
    /// </summary>
    public event Action Created;

    /// <summary>
    /// Called when an instance is just about to be destroyed.
    /// </summary>
    public event Action Destroyed;

    /// <summary>
    /// <c>Start</c> has been called on an event instance which was not already playing. The event will remain in this state until its sample data has been loaded.
    /// </summary>
    public event Action Starting;

    /// <summary>
    /// The event has commenced playing. Normally this callback will be issued immediately after <c>Starting</c>, but may be delayed until sample data has loaded.
    /// </summary>
    public event Action Started;

    /// <summary>
    /// <c>Start</c> has been called on an event instance which was already playing.
    /// </summary>
    public event Action Restarted;

    /// <summary>
    /// The event has stopped.
    /// </summary>
    public event Action Stopped;

    /// <summary>
    /// <c>Start</c> has been called but the polyphony settings did not allow the event to start.
    /// In this case none of <c>Starting</c>, <c>Started</c> and <c>Stopped</c> will be called.
    /// </summary>
    public event Action StartFailed;

    /// <summary>
    /// A programmer sound is about to play.
    /// </summary>
    public event Action<PROGRAMMER_SOUND_PROPERTIES> ProgrammerSoundCreated;

    /// <summary>
    /// A programmer sound has stopped playing.
    /// </summary>
    public event Action<PROGRAMMER_SOUND_PROPERTIES> ProgrammerSoundDestroyed;

    /// <summary>
    /// Called when a DSP plugin instance has just been created.
    /// </summary>
    public event Action<PLUGIN_INSTANCE_PROPERTIES> PluginCreated;

    /// <summary>
    /// Called when a DSP plugin instance is about to be destroyed.
    /// </summary>
    public event Action<PLUGIN_INSTANCE_PROPERTIES> PluginDestroyed;

    /// <summary>
    /// Called when the timeline passes a named marker.
    /// </summary>
    public event Action<TIMELINE_MARKER_PROPERTIES> TimelineMarketHit;

    /// <summary>
    /// Called when the timeline hits a beat in a tempo section.
    /// </summary>
    public event Action<TIMELINE_BEAT_PROPERTIES> TimelineBeatHit;

    /// <summary>
    /// Called when the event plays a sound.
    /// </summary>
    public event Action<Sound> SoundPlayed;

    /// <summary>
    /// Called when the event finishes playing a sound.
    /// </summary>
    public event Action<Sound> SoundStopped;

    /// <summary>
    /// Called when the event becomes virtual. <see href="https://www.fmod.com/docs/2.03/api/white-papers-virtual-voices.html">See here for more info</see>
    /// </summary>
    public event Action RealToVirtual;

    /// <summary>
    /// Called when the event becomes real. <see href="https://www.fmod.com/docs/2.03/api/white-papers-virtual-voices.html">See here for more info</see>
    /// </summary>
    public event Action VirtualToReal;

    /// <summary>
    /// Called when a new event is started by a start event command.
    /// </summary>
    public event Action<EventInstance> EventCommandStarted;

    /// <summary>
    /// Called when the timeline hits a beat in a tempo section of a nested event.
    /// </summary>
    public event Action<TIMELINE_NESTED_BEAT_PROPERTIES> NestedTimelineBeatHit;

    private EVENT_CALLBACK eventCallback;

    public FMODEventInstance(FMODEvent @event)
        : this(@event.Description) { }

    public FMODEventInstance(EventDescription eventDescription)
    {
        Description = eventDescription;
        FMODCaller.CheckResult(Description.createInstance(out Instance));

        eventCallback = (type, eventPointer, parameterPointer) =>
        {
            switch (type)
            {
                case EVENT_CALLBACK_TYPE.CREATED:
                    Created?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.DESTROYED:
                    Destroyed?.Invoke();
                    IsReleased = true;
                    break;
                case EVENT_CALLBACK_TYPE.STARTING:
                    Starting?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.STARTED:
                    Started?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.RESTARTED:
                    Restarted?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.STOPPED:
                    Stopped?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.START_FAILED:
                    StartFailed?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                    unsafe
                    {
                        var properties = (PROGRAMMER_SOUND_PROPERTIES*)parameterPointer.ToPointer();
                        ProgrammerSoundCreated?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                    unsafe
                    {
                        var properties = (PROGRAMMER_SOUND_PROPERTIES*)parameterPointer.ToPointer();
                        ProgrammerSoundDestroyed?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.PLUGIN_CREATED:
                    unsafe
                    {
                        var properties = (PLUGIN_INSTANCE_PROPERTIES*)parameterPointer.ToPointer();
                        PluginCreated?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.PLUGIN_DESTROYED:
                    unsafe
                    {
                        var properties = (PLUGIN_INSTANCE_PROPERTIES*)parameterPointer.ToPointer();
                        PluginDestroyed?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    unsafe
                    {
                        var properties = (TIMELINE_MARKER_PROPERTIES*)parameterPointer.ToPointer();
                        TimelineMarketHit?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    unsafe
                    {
                        var properties = (TIMELINE_BEAT_PROPERTIES*)parameterPointer.ToPointer();
                        TimelineBeatHit?.Invoke(*properties);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.SOUND_PLAYED:
                    unsafe
                    {
                        var sound = (Sound*)parameterPointer.ToPointer();
                        SoundPlayed?.Invoke(*sound);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.SOUND_STOPPED:
                    unsafe
                    {
                        var sound = (Sound*)parameterPointer.ToPointer();
                        SoundStopped?.Invoke(*sound);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.REAL_TO_VIRTUAL:
                    RealToVirtual?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.VIRTUAL_TO_REAL:
                    VirtualToReal?.Invoke();
                    break;
                case EVENT_CALLBACK_TYPE.START_EVENT_COMMAND:
                    unsafe
                    {
                        var eventInstance = (EventInstance*)parameterPointer.ToPointer();
                        EventCommandStarted?.Invoke(*eventInstance);
                    }
                    break;
                case EVENT_CALLBACK_TYPE.NESTED_TIMELINE_BEAT:
                    unsafe
                    {
                        var properties = (TIMELINE_NESTED_BEAT_PROPERTIES*)
                            parameterPointer.ToPointer();
                        NestedTimelineBeatHit?.Invoke(*properties);
                    }
                    break;
            }
            ;
            return RESULT.OK;
        };
        SetCallback(EVENT_CALLBACK_TYPE.ALL, eventCallback);
    }

    /// <summary>
    /// Starts playback of the event instance.
    /// </summary>
    public void Start()
    {
        FMODCaller.CheckResult(Instance.start());
    }

    /// <summary>
    /// Attaches a 3D node to the event instance.
    /// This means that the 3D attributes of the event instances will be updated according to the node's position.
    /// </summary>
    public void AttachNode(Node3D node)
    {
        if (AttachedNode == null)
            Defluo.FMOD.AddAttachedEventInstance(this);
        AttachedNode = node;
    }

    /// <summary>
    /// Updates the 3D attributes of event instance. This is called automatically from the manager node.
    /// </summary>
    public void Update3DAttributes(ATTRIBUTES_3D attributes)
    {
        FMODCaller.CheckResult(Instance.set3DAttributes(attributes));
    }

    /// <summary>
    /// Stops the event playback.
    /// </summary>
    /// <param name="allowFadeout">If true, the event is allowed to fade out.</param>
    public void Stop(bool allowFadeout = true)
    {
        FMODCaller.CheckResult(
            Instance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE)
        );
    }

    /// <summary>
    /// Releases the event instance from memory. This should always be called when event instance is no longer needed.
    /// </summary>
    public void Release()
    {
        FMODCaller.CheckResult(Instance.release());
    }

    /// <summary>
    /// Manually registers a callback function for specified events.
    /// </summary>
    public void SetCallback(EVENT_CALLBACK_TYPE callbackMask, EVENT_CALLBACK callback)
    {
        FMODCaller.CheckResult(Instance.setCallback(callback, callbackMask));
    }

    /// <summary>
    /// Returns the current value of parameter
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetParameter(string name)
    {
        FMODCaller.CheckResult(Instance.getParameterByName(name, out var value));
        return value;
    }

    /// <summary>
    /// Returns the current value of parameter as a string label
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetParameterAsLabel(string name)
    {
        var parameter = new FMODParameter(name);

        if (!parameter.IsLabeled)
            throw new Exception("Parameter is not labeled");

        var value = GetParameter(name);
        return parameter.Labels[(int)value];
    }

    /// <summary>
    /// Sets the value of a <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameter</see>.
    /// </summary>
    /// <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetParameter(FMODParameter parameter, float value, bool ignoreSeek = false) =>
        SetParameter(parameter.Path, value, ignoreSeek);

    /// <summary>
    /// Sets the value of a <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameter</see>.
    /// </summary>
    /// <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetParameter(string name, float value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(Instance.setParameterByName(name, value, ignoreSeek));
    }

    /// <summary>
    /// Sets the value of a <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameter</see>.
    /// </summary>
    ///  <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetParameter(FMODParameter parameter, string value, bool ignoreSeek = false) =>
        SetParameter(parameter.Path, value, ignoreSeek);

    /// <summary>
    /// Sets the value of a <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameter</see>.
    /// </summary>
    /// <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetParameter(string name, string value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(Instance.setParameterByNameWithLabel(name, value, ignoreSeek));
    }

    /// <summary>
    /// Sets the value of <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameters</see>
    /// based on the given list of parameters and their desired values.
    /// This is useful for quick initialization of an event for playback. The parameter's seek speed is ignored.
    /// </summary>
    public void SetParameters(params (string name, Variant value)[] parameters)
    {
        foreach (var (name, value) in parameters)
        {
            if (value.VariantType == Variant.Type.String)
                SetParameter(name, value.As<string>(), true);
            else
                SetParameter(name, value.As<float>(), true);
        }
    }
}
