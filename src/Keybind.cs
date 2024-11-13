namespace DefluoLib;

using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// A pre-defined input action that can be subscribed to with callback functions.
/// Define one as a property in a partial class declaration to register it to the settings.
/// </summary>
public class Keybind
{
    /// <summary>
    /// Default value for axis threshold
    /// </summary>
    public const float DEFAULT_AXIS_THRESHOLD = 0.9f;

    /// <summary>
    /// Name of keybind, displayed in the settings menu
    /// </summary>
    public string DisplayName { get; internal set; }

    /// <summary>
    /// Category of keybind. Empty string if not defined.
    /// Categories can be defined by adding a <c>CreateCategory</c> attribute to keybind definition
    /// </summary>
    public string Category { get; internal set; } = "";

    /// <summary>
    /// If keybind is currently pressed down
    /// </summary>
    public bool IsPressed { get; internal set; }

    /// <summary>
    /// Amount of process frames that keybind has been held down
    /// </summary>
    public int ProcessFramesHeldDown { get; internal set; }

    /// <summary>
    /// Amount of physics frames that keybind has been held down
    /// </summary>
    public int PhysicsFramesHeldDown { get; internal set; }

    internal bool processIsJustPressed;
    internal bool processIsJustReleased;
    internal bool physicsIsJustPressed;
    internal bool physicsIsJustReleased;

    /// <summary>
    /// If keybind has been pressed down this frame
    /// </summary>
    public bool IsJustPressed => Engine.IsInPhysicsFrame() ? physicsIsJustPressed : processIsJustPressed;

    /// <summary>
    /// If keybind has been released this frame
    /// </summary>
    public bool IsJustReleased => Engine.IsInPhysicsFrame() ? physicsIsJustReleased : processIsJustReleased;

    /// <summary>
    /// List of inputs binded to this action,
    /// represented as input enums
    /// </summary>
    public List<DigitalInput> BindedInputs { get; internal set; }

    public KeybindOptions Options { get; internal set; }

    /// <summary>
    /// Activate the action, invoking correct event
    /// </summary>
    public void Activate(bool pressed)
    {
        IsPressed = pressed;
        if (pressed)
            Pressed?.Invoke();
        else
            Released?.Invoke();
    }

    internal void SetCategory(string category)
    {
        Category = category;
    }

    /// <summary>
    /// Fired when keybind is pressed
    /// </summary>
    public event Action Pressed;

    /// <summary>
    /// Fired when keybind is released
    /// </summary>
    public event Action Released;

    public Keybind(string displayName, KeybindOptions options, params DigitalInput[] inputs)
    {
        DisplayName = displayName;
        Options = options;
        BindedInputs = [.. inputs];
    }

    public Keybind(string displayName, params DigitalInput[] inputs)
        : this(displayName, new(), inputs) { }
}

/// <summary>
/// Class containing possible options for keybinds
/// </summary>
public class KeybindOptions
{
    /// <summary>
    /// Threshold for when an analog input is determined to in the pressed state.
    /// Used only if axis inputs have been binded to this keybind
    /// </summary>
    public float AxisThreshold;

    /// <summary>
    /// Essential keybinds will remain functional even when disabling input processing
    /// </summary>
    public bool IsEssential;

    /// <summary>
    /// If keybind can be rebinded from the in-game keybinds menu
    /// </summary>
    public bool CanBeRebinded;

    /// <summary>
    ///
    /// </summary>
    /// <param name="isEssential">Essential keybinds will remain functional even when disabling input processing</param>
    /// <param name="canBeRebinded">If keybind can be rebinded from the in-game keybinds menu</param>
    /// <param name="axisThreshold">
    /// Threshold for when an analog input is determined to in the pressed state.
    /// Used only if axis inputs have been binded to this keybind
    /// </param>
    public KeybindOptions(
        bool isEssential = false,
        bool canBeRebinded = true,
        float axisThreshold = Keybind.DEFAULT_AXIS_THRESHOLD
    )
    {
        AxisThreshold = axisThreshold;
        IsEssential = isEssential;
        CanBeRebinded = canBeRebinded;
    }
}
