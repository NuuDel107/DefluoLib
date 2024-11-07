using Godot;
using FMOD;
using FMOD.Studio;
using System;

namespace DefluoLib;

public class FMODEventInstance
{
    public EventInstance Instance;
    public EventDescription Description;
    public Node3D AttachedNode;

    public PLAYBACK_STATE PlaybackState
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPlaybackState(out var state));
            return state;
        }
    }

    public bool Paused
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPaused(out var paused));
            return paused;
        }
        set => FMODCaller.CheckResult(Instance.setPaused(value));
    }

    public float Volume
    {
        get
        {
            FMODCaller.CheckResult(Instance.getVolume(out var volume));
            return volume;
        }
        set => FMODCaller.CheckResult(Instance.setVolume(value));
    }

    public float Pitch
    {
        get
        {
            FMODCaller.CheckResult(Instance.getPitch(out var pitch));
            return pitch;
        }
        set => FMODCaller.CheckResult(Instance.setPitch(value));
    }

    public bool IsPlaying => PlaybackState == PLAYBACK_STATE.PLAYING;

    public event Action Created;
    public event Action Destroyed;
    public event Action Starting;
    public event Action Started;
    public event Action Restarted;
    public event Action Stopped;
    public event Action StartFailed;
    public event Action<PROGRAMMER_SOUND_PROPERTIES> ProgrammerSoundCreated;
    public event Action<PROGRAMMER_SOUND_PROPERTIES> ProgrammerSoundDestroyed;
    public event Action<PLUGIN_INSTANCE_PROPERTIES> PluginCreated;
    public event Action<PLUGIN_INSTANCE_PROPERTIES> PluginDestroyed;
    public event Action<TIMELINE_MARKER_PROPERTIES> TimelineMarketHit;
    public event Action<TIMELINE_BEAT_PROPERTIES> TimelineBeatHit;
    public event Action<Sound> SoundPlayed;
    public event Action<Sound> SoundStopped;
    public event Action RealToVirtual;
    public event Action VirtualToReal;
    public event Action<EventInstance> EventCommandStarted;
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

    public void Start()
    {
        FMODCaller.CheckResult(Instance.start());
    }

    public void AttachNode(Node3D node)
    {
        if (AttachedNode == null)
            Defluo.FMOD.AddAttachedEventInstance(this);
        AttachedNode = node;
    }

    public void Update3DAttributes(ATTRIBUTES_3D attributes)
    {
        FMODCaller.CheckResult(Instance.set3DAttributes(attributes));
    }

    public void Stop(bool allowFadeout = true)
    {
        FMODCaller.CheckResult(
            Instance.stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE)
        );
    }

    public void Release()
    {
        FMODCaller.CheckResult(Instance.release());
    }

    public void SetCallback(EVENT_CALLBACK_TYPE callbackMask, EVENT_CALLBACK callback)
    {
        FMODCaller.CheckResult(Instance.setCallback(callback, callbackMask));
    }

    public void SetParameter(FMODParameter parameter, float value, bool ignoreSeek = false) =>
        SetParameter(parameter.Path, value, ignoreSeek);

    public void SetParameter(string name, float value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(Instance.setParameterByName(name, value, ignoreSeek));
    }

    public void SetParameter(FMODParameter parameter, string value, bool ignoreSeek = false) =>
        SetParameter(parameter.Path, value, ignoreSeek);

    public void SetParameter(string name, string value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(Instance.setParameterByNameWithLabel(name, value, ignoreSeek));
    }

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
