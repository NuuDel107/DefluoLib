namespace DefluoLib;

using Godot;
using System;

public abstract class AnimationParameter(AnimationTree animTree) : IDisposable
{
    protected readonly AnimationTree animationTree = animTree;

    public void Dispose() => GC.SuppressFinalize(this);
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeoneshot.html">AnimationNodeOneShot</see>
/// </summary>
public class OneshotAnimationParameter : AnimationParameter
{
    public StringName RequestPath;
    public StringName ActivePath;
    public AnimationNodeOneShot Node;

    public OneshotAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        RequestPath = new StringName($"parameters/{parameterName}/request");
        ActivePath = new StringName($"parameters/{parameterName}/active");
        Node = (AnimationNodeOneShot)animationTree.Get($"parameters/{parameterName}");
    }

    public void Fire() =>
        animationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.Fire);

    public void Abort() =>
        animationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.Abort);

    public void FadeOut() =>
        animationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);

    public bool IsActive => (bool)animationTree.Get(ActivePath);
}

/// <summary>
/// Type of animation node the parameter wrapper represents
/// </summary>
public enum BlendAnimationParameterType
{
    /// <summary>
    /// Represents <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblend2.html">Blend2</see>
    /// and <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblend3.html">Blend3</see> nodes
    /// </summary>
    Blend,

    /// <summary>
    /// Represents <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeadd2.html">Add2</see>
    /// and <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeadd3.html">Add3</see> nodes
    /// </summary>
    Add,

    /// <summary>
    /// Represents <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodesub2.html">Sub2</see> node
    /// </summary>
    Sub
}

/// <summary>
/// Parameter wrapper for different blend animation nodes
/// </summary>
public class BlendAnimationParameter : AnimationParameter
{
    public StringName Path;

    public BlendAnimationParameter(
        AnimationTree animTree,
        string parameterName,
        BlendAnimationParameterType type
    )
    : base(animTree)
    {
        switch (type)
        {
            case BlendAnimationParameterType.Blend:
                Path = new StringName($"parameters/{parameterName}/blend_amount");
                break;
            case BlendAnimationParameterType.Add:
                Path = new StringName($"parameters/{parameterName}/add_amount");
                break;
            case BlendAnimationParameterType.Sub:
                Path = new StringName($"parameters/{parameterName}/sub_amount");
                break;
        }
    }

    public float Amount
    {
        get => (float)animationTree.Get(Path);
        set => animationTree.Set(Path, value);
    }
}

/// <summary>
/// Condition wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodestatemachine.html">AnimationNodeStateMachine</see>
/// </summary>
public class StateMachineAnimationParameter : AnimationParameter
{
    public StringName ConditionPath;
    public AnimationNodeStateMachine Node;
    public AnimationNodeStateMachinePlayback Playback;

    public StateMachineAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        ConditionPath = new StringName($"parameters/{parameterName}/");
        Playback = (AnimationNodeStateMachinePlayback)
            animationTree.Get($"parameters/{parameterName}/playback");
        Node = (AnimationNodeStateMachine)animationTree.Get($"parameters/{parameterName}");
    }

    public void SetCondition(string conditionName, bool value) =>
        animationTree.Set(ConditionPath + conditionName, value);

    public void GetCondition(string conditionName) =>
        animationTree.Get(ConditionPath + conditionName);
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblendspace1d.html">AnimationNodeBlendSpace1D</see>
/// </summary>
public class BlendSpace1DAnimationParameter : AnimationParameter
{
    public StringName Path;
    public AnimationNodeBlendSpace1D Node;

    public BlendSpace1DAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        Path = new StringName($"parameters/{parameterName}/blend_position");
        Node = (AnimationNodeBlendSpace1D)animationTree.Get($"parameters/{parameterName}");
    }

    public float Position
    {
        get => (float)animationTree.Get(Path);
        set => animationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblendspace2d.html">AnimationNodeBlendSpace2D</see>
/// </summary>
public class BlendSpace2DAnimationParameter : AnimationParameter
{
    public StringName Path;
    public AnimationNodeBlendSpace2D Node;

    public BlendSpace2DAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        Path = new StringName($"parameters/{parameterName}/blend_position");
        Node = (AnimationNodeBlendSpace2D)animationTree.Get($"parameters/{parameterName}");
    }

    public Vector2 Position
    {
        get => (Vector2)animationTree.Get(Path);
        set => animationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetimescale.html">AnimationNodeTimeScale</see>
/// </summary>
public class TimeScaleAnimationParameter : AnimationParameter
{
    public StringName Path;

    public TimeScaleAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        Path = new StringName($"parameters/{parameterName}/scale");
    }

    public float Scale
    {
        get => (float)animationTree.Get(Path);
        set => animationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetimeseek.html">AnimationNodeTimeSeek</see>
/// </summary>
public class TimeSeekAnimationParameter : AnimationParameter
{
    public StringName RequestPath;

    public TimeSeekAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        RequestPath = new StringName($"parameters/{parameterName}/seek_request");
    }

    public void Seek(float seekValue) => animationTree.Set(RequestPath, seekValue);
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetransition.html">AnimationNodeTransition</see>
/// </summary>
public class TransitionAnimationParameter : AnimationParameter
{
    public StringName RequestPath;
    public StringName StatePath;
    public StringName IndexPath;
    public AnimationNodeTransition Node;

    public TransitionAnimationParameter(AnimationTree animTree, string parameterName)
        : base(animTree)
    {
        RequestPath = new StringName($"parameters/{parameterName}/transition_request");
        StatePath = new StringName($"parameters/{parameterName}/current_state");
        IndexPath = new StringName($"parameters/{parameterName}/current_index");
        Node = (AnimationNodeTransition)animationTree.Get($"parameters/{parameterName}");
    }

    public void Transition(string state) => animationTree.Set(RequestPath, state);

    public string CurrentState => (string)animationTree.Get(StatePath);

    public string CurrentIndex => (string)animationTree.Get(IndexPath);
}
