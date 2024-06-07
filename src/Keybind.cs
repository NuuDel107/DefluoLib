using System;
using System.Collections.Generic;
using System.Linq;

namespace DefluoLib;

/// <summary>
/// A pre-defined input action that can be subscribed to with callback functions.
/// Define one as a property in a partial class declaration to register it to the settings.
/// </summary>
public class Keybind
{
    /// <summary>
    /// Default value for axis threshold
    /// </summary>
    public const float DefaultAxisThreshold = 0.9f;

    /// <summary>
    /// Name of keybind, displayed in the settings menu
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// Category of keybind. Empty string if not defined.
    /// Categories can be defined by adding a <c>CreateCategory</c> attribute to keybind definition
    /// </summary>
    public string Category = "";

    /// <summary>
    /// If keybind is currently pressed down
    /// </summary>
    public bool IsPressed = false;

    /// <summary>
    /// List of inputs binded to this action,
    /// represented as input enums
    /// </summary>
    public List<DigitalInput> BindedInputs;

    public KeybindOptions Options;

    private List<Action> onPressedActions = new();
    private List<Action> onReleasedActions = new();

    /// <summary>
    /// Activate the action, running all functions that are subscribed to it
    /// </summary>
    public void Activate(bool pressed)
    {
        IsPressed = pressed;
        if (pressed)
            foreach (var action in onPressedActions)
                action();
        else
            foreach (var action in onReleasedActions)
                action();
    }

    internal void SetCategory(string category)
    {
        Category = category;
    }

    /// <summary>
    /// Define a function that is ran when the keybind is pressed
    /// </summary>
    public void OnPressed(Action action)
    {
        onPressedActions.Add(action);
    }

    /// <summary>
    /// Define a function that is ran when the keybind is released
    /// </summary>
    public void OnReleased(Action action)
    {
        onPressedActions.Add(action);
    }

    public Keybind(
        string displayName,
        KeybindOptions options,
        params DigitalInput[] inputs
    )
    {
        DisplayName = displayName;
        Options = options;
        BindedInputs = inputs.ToList();
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
    public KeybindOptions(bool isEssential = false, bool canBeRebinded = true, float axisThreshold = Keybind.DefaultAxisThreshold)
    {
        AxisThreshold = axisThreshold;
        IsEssential = isEssential;
        CanBeRebinded = canBeRebinded;
    }
}
