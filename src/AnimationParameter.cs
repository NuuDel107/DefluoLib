namespace DefluoLib;

using Godot;

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeoneshot.html">AnimationNodeOneShot</see>
/// </summary>
public class OneshotAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName RequestPath;
    public StringName ActivePath;
    public AnimationNodeOneShot Node;

    public OneshotAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        RequestPath = new StringName($"parameters/{parameterName}/request");
        ActivePath = new StringName($"parameters/{parameterName}/active");
        Node = (AnimationNodeOneShot)AnimationTree.Get($"parameters/{parameterName}");
    }

    public void Fire() =>
        AnimationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.Fire);

    public void Abort() =>
        AnimationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.Abort);

    public void FadeOut() =>
        AnimationTree.Set(RequestPath, (int)AnimationNodeOneShot.OneShotRequest.FadeOut);

    public bool IsActive
    {
        get => (bool)AnimationTree.Get(ActivePath);
    }
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
public class BlendAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName Path;

    public BlendAnimationParameter(
        AnimationTree animationTree,
        string parameterName,
        BlendAnimationParameterType type
    )
    {
        AnimationTree = animationTree;
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
        get => (float)AnimationTree.Get(Path);
        set => AnimationTree.Set(Path, value);
    }
}

/// <summary>
/// Condition wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodestatemachine.html">AnimationNodeStateMachine</see>
/// </summary>
public class StateMachineAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName ConditionPath;
    public AnimationNodeStateMachine Node;
    public AnimationNodeStateMachinePlayback Playback;

    public StateMachineAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        ConditionPath = new StringName($"parameters/{parameterName}/");
        Playback = (AnimationNodeStateMachinePlayback)
            AnimationTree.Get($"parameters/{parameterName}/playback");
        Node = (AnimationNodeStateMachine)AnimationTree.Get($"parameters/{parameterName}");
    }

    public void SetCondition(string conditionName, bool value) =>
        AnimationTree.Set(ConditionPath + conditionName, value);

    public void GetCondition(string conditionName) =>
        AnimationTree.Get(ConditionPath + conditionName);
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblendspace1d.html">AnimationNodeBlendSpace1D</see>
/// </summary>
public class BlendSpace1DAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName Path;
    public AnimationNodeBlendSpace1D Node;

    public BlendSpace1DAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        Path = new StringName($"parameters/{parameterName}/blend_position");
        Node = (AnimationNodeBlendSpace1D)AnimationTree.Get($"parameters/{parameterName}");
    }

    public float Position
    {
        get => (float)AnimationTree.Get(Path);
        set => AnimationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodeblendspace2d.html">AnimationNodeBlendSpace2D</see>
/// </summary>
public class BlendSpace2DAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName Path;
    public AnimationNodeBlendSpace2D Node;

    public BlendSpace2DAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        Path = new StringName($"parameters/{parameterName}/blend_position");
        Node = (AnimationNodeBlendSpace2D)AnimationTree.Get($"parameters/{parameterName}");
    }

    public Vector2 Position
    {
        get => (Vector2)AnimationTree.Get(Path);
        set => AnimationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetimescale.html">AnimationNodeTimeScale</see>
/// </summary>
public class TimeScaleAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName Path;

    public TimeScaleAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        Path = new StringName($"parameters/{parameterName}/scale");
    }

    public float Scale
    {
        get => (float)AnimationTree.Get(Path);
        set => AnimationTree.Set(Path, value);
    }
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetimeseek.html">AnimationNodeTimeSeek</see>
/// </summary>
public class TimeSeekAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName RequestPath;

    public TimeSeekAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        RequestPath = new StringName($"parameters/{parameterName}/seek_request");
    }

    public void Seek(float seekValue) => AnimationTree.Set(RequestPath, seekValue);
}

/// <summary>
/// Parameter wrapper for <see href="https://docs.godotengine.org/en/stable/classes/class_animationnodetransition.html">AnimationNodeTransition</see>
/// </summary>
public class TransitionAnimationParameter
{
    private AnimationTree AnimationTree;
    public StringName RequestPath;
    public StringName StatePath;
    public StringName IndexPath;
    public AnimationNodeTransition Node;

    public TransitionAnimationParameter(AnimationTree animationTree, string parameterName)
    {
        AnimationTree = animationTree;
        RequestPath = new StringName($"parameters/{parameterName}/transition_request");
        StatePath = new StringName($"parameters/{parameterName}/current_state");
        IndexPath = new StringName($"parameters/{parameterName}/current_index");
        Node = (AnimationNodeTransition)AnimationTree.Get($"parameters/{parameterName}");
    }

    public void Transition(string state) => AnimationTree.Set(RequestPath, state);

    public string CurrentState
    {
        get => (string)AnimationTree.Get(StatePath);
    }

    public string CurrentIndex
    {
        get => (string)AnimationTree.Get(IndexPath);
    }
}
