using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DefluoLib;

/// <summary>
/// Used to interface with mouse movement
/// </summary>
public class Mouse
{
    /// <summary>
    /// Current position of mouse pointer in viewport
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Position of mouse pointer in viewport relative to previous mouse position change
    /// </summary>
    public Vector2 RelativePosition { get; private set; }

    private List<Action<Vector2>> movementActions = new();
    private List<Action<Vector2>> relativeMovementActions = new();

    /// <summary>
    /// Update position of mouse and run defined functions
    /// </summary>
    /// <param name="pos">Current position of mouse in viewport</param>
    /// <param name="relativePos">Position of mouse in relation to the last frame</param>
    internal void UpdatePosition(Vector2 pos, Vector2 relativePos)
    {
        Position = pos;
        RelativePosition = relativePos;

        foreach (var function in movementActions)
        {
            function(pos);
        }

        foreach (var function in relativeMovementActions)
        {
            function(relativePos);
        }
    }

    /// <summary>
    /// Sets the mouse position to specified vector.
    /// </summary>
    /// <param name="position"></param>
    public void Warp(Vector2 position) => Godot.Input.WarpMouse(position);

    /// <summary>
    /// Define a function that is ran when the mouse is moved
    /// </summary>
    /// <param name="action">
    ///     Function that is ran on mouse movement.
    ///     The parameter represents the current position of the mouse in viewport
    /// </param>
    public void OnMovement(Action<Vector2> action)
    {
        movementActions.Add(action);
    }

    /// <summary>
    /// Define a function that is ran when the mouse is moved
    /// </summary>
    /// <param name="action">
    ///     Function that is ran on mouse movement.
    ///     The parameter represents the position of the mouse in viewport relative to last frame
    /// </param>
    public void OnRelativeMovement(Action<Vector2> action)
    {
        relativeMovementActions.Add(action);
    }
}

/// <summary>
/// Node that handles access to input actions and updates their states
/// </summary>
public partial class Input : Node
{
    public const string UserKeybindsResourcePath = "user://keybinds.tres";

    /// <summary>
    /// If <c>true</c>, all input events will be logged to the console
    /// </summary>
    public bool InputLogging = (bool)
        ProjectSettings.Singleton.GetSetting("DefluoLib/Input/InputLogging");

    /// <summary>
    /// If <c>true</c>, triggered actions will be logged to the console
    /// </summary>
    public bool EventLogging = (bool)
        ProjectSettings.Singleton.GetSetting("DefluoLib/Input/EventLogging");

    public Mouse Mouse { get; set; } = new();

    /// <summary>
    /// If <c>true</c>, the cursor is being captured by the game window
    /// </summary>
    public bool IsMouseCaptured
    {
        get { return Godot.Input.MouseMode == Godot.Input.MouseModeEnum.Captured; }
        set
        {
            Godot.Input.MouseMode = value
                ? Godot.Input.MouseModeEnum.Captured
                : Godot.Input.MouseModeEnum.Visible;
        }
    }

    /// <summary>
    /// If input class is currently disabled,
    /// meaning non-essential actions are never triggered.
    /// Use this to
    /// </summary>
    private bool disabled;
    public bool Disabled
    {
        get { return disabled; }
        set
        {
            // When disabling input, reset press states
            if (value)
            {
                foreach (var keybind in keybinds)
                {
                    // Don't reset essential keybinds
                    if (!keybind.IsEssential)
                        keybind.IsPressed = false;
                }
            }

            if (EventLogging)
                GD.Print(value ? "Input disabled" : "Input enabled");

            disabled = value;
        }
    }

    private List<PropertyInfo> properties;
    private List<Keybind> keybinds;
    private DictionaryResource defaultKeybindResource;
    private DictionaryResource keybindResource;

    public override void _EnterTree()
    {
        Name = "Input";
        properties = GetKeybindProperties();
        keybinds = GetKeybinds();

        // Loop through keybinds and their properties to set categories for them
        // based on declared attributes
        CreateCategoryAttribute latestCategoryAttribute = null;
        foreach (var (keybind, index) in keybinds.WithIndex())
        {
            var attribute = properties[index].GetCustomAttribute(typeof(CreateCategoryAttribute));
            if (attribute != null)
                latestCategoryAttribute = (CreateCategoryAttribute)attribute;
            if (latestCategoryAttribute != null)
                keybind.SetCategory(latestCategoryAttribute.DisplayName);
        }

        // Create a default settings resource by running resource creation function
        // using settings with default values before they get overwritten by initialization
        defaultKeybindResource = CreateResourceFromValues();

        if (ResourceLoader.Exists(UserKeybindsResourcePath))
        {
            keybindResource = ResourceLoader.Load<DictionaryResource>(UserKeybindsResourcePath);
            LoadKeybinds(keybindResource);
        }
        else
        {
            GD.Print("User keybinds not found, using default values");
            keybindResource = defaultKeybindResource;
        }
        SaveKeybinds();
    }

    /// <summary>
    /// Returns a list of defined keybind properties
    /// </summary>
    /// <returns></returns>
    internal static List<PropertyInfo> GetKeybindProperties()
    {
        IEnumerable<PropertyInfo> list =
            from property in typeof(Input).GetProperties()
            where typeof(Keybind).IsAssignableFrom(property.PropertyType)
            select property;

        return list.ToList();
    }

    /// <summary>
    /// Returns a list of defined keybinds
    /// </summary>
    public List<Keybind> GetKeybinds()
    {
        List<Keybind> list = properties
            .Select(property => (Keybind)property.GetValue(this))
            .ToList();

        return list.ToList();
    }

    /// <summary>
    /// Event that is invoked when a digital input is pressed
    /// </summary>
    public event Action<DigitalInput> DigitalInputPressed;

    /// <summary>
    /// Waits for a digital input to be pressed, and returns it
    /// </summary>
    /// <returns></returns>
    public Task<DigitalInput> AwaitForDigitalInput()
    {
        var result = new TaskCompletionSource<DigitalInput>();
        void eventHandler(DigitalInput input)
        {
            result.SetResult(input);
            DigitalInputPressed -= eventHandler;
        }

        DigitalInputPressed += eventHandler;
        return result.Task;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (InputLogging)
            GD.Print(@event.AsText());

        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (!Disabled)
                // Update mouse class with the position and relative position vectors
                Mouse.UpdatePosition(mouseMotionEvent.Position, mouseMotionEvent.Relative);
        }
        else
        {
            // Loop through keybinds
            foreach (var keybind in keybinds)
            {
                // If input is disabled, only run actions for essential keybinds
                if (Disabled && !keybind.IsEssential)
                    continue;

                // See if event key has been binded to an action
                foreach (var bindedInput in keybind.BindedInputs)
                {
                    if (
                        @event is InputEventKey keyEvent
                        && bindedInput.Equals(keyEvent.PhysicalKeycode)
                    )
                    {
                        HandleDigitalPress(keybind, keyEvent.Pressed);
                        break;
                    }
                    else if (
                        @event is InputEventMouseButton mouseButtonEvent
                        && bindedInput.Equals(mouseButtonEvent.ButtonIndex)
                    )
                    {
                        HandleDigitalPress(keybind, mouseButtonEvent.Pressed);
                        break;
                    }
                    else if (
                        @event is InputEventJoypadButton joypadEvent
                        && bindedInput.Equals(joypadEvent.ButtonIndex)
                    )
                    {
                        HandleDigitalPress(keybind, joypadEvent.Pressed);
                        break;
                    }
                    else if (
                        @event is InputEventJoypadMotion joypadMotionEvent
                        && bindedInput.Equals(joypadMotionEvent.Axis)
                    )
                    {
                        HandleAnalogPress(
                            keybind,
                            joypadMotionEvent.AxisValue,
                            keybind.AxisThreshold
                        );
                        break;
                    }
                }
            }

            // If analog value is below the digital input threshold,
            // don't register it as a digital input event
            if (
                @event is InputEventJoypadMotion analogEvent
                && Mathf.Abs(analogEvent.AxisValue) < Keybind.DefaultAxisThreshold
            )
                return;

            // Parse DigitalInput from input event
            var input = DigitalInput.FromEvent(@event);
            GD.Print(input.DisplayString);
            // Invoke input event if input could be parsed
            if (DigitalInputPressed != null && input != null)
                DigitalInputPressed.Invoke(input);
        }
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationApplicationFocusOut)
        {
            foreach (var keybind in keybinds)
            {
                if (keybind.IsPressed)
                {
                    keybind.Activate(false);
                }
            }
        }
    }

    /// <summary>
    /// Returns the current pressed state of digital input
    /// </summary>
    /// <param name="input">Digital input whose state is retrieved</param>
    /// <param name="device">Device index of target controller, used only for controller input</param>
    /// <param name="axisThreshold">Threshold for when an analog input is determined to in the pressed state</param>
    /// <returns></returns>
    public bool GetDigitalValue(
        DigitalInput input,
        int device = 0,
        float axisThreshold = Keybind.DefaultAxisThreshold
    )
    {
        if (Disabled)
            return false;

        return input.Type switch
        {
            DigitalInputType.Key => Godot.Input.IsKeyPressed((Key)input.EnumValue),
            DigitalInputType.MouseButton
                => Godot.Input.IsMouseButtonPressed((MouseButton)input.EnumValue),
            DigitalInputType.JoyButton
                => Godot.Input.IsJoyButtonPressed(device, (JoyButton)input.EnumValue),
            DigitalInputType.JoyAxis
                => input.PositiveAnalog
                    ? Godot.Input.GetJoyAxis(device, (JoyAxis)input.EnumValue) > axisThreshold
                    : Godot.Input.GetJoyAxis(device, (JoyAxis)input.EnumValue) < -axisThreshold,
            _ => false
        };
    }

    /// <summary>
    /// Returns the current value of analog input
    /// </summary>
    /// <param name="input">Analog input whose value is retrieved</param>
    /// <param name="device">Device index of target controller, used only for controller input</param>
    /// <returns></returns>
    public float GetAnalogValue(AnalogInput input, int device = 0) =>
        Godot.Input.GetJoyAxis(device, input.Axis);

    /// <summary>
    /// Returns a normalized vector with values mapped to it
    /// depending on whether the passed keybinds are pressed or not
    /// </summary>
    public Vector2 GetVector(
        Keybind negativeX,
        Keybind positiveX,
        Keybind negativeY,
        Keybind positiveY
    )
    {
        if (Disabled)
            return Vector2.Zero;
        return new Vector2(
            (positiveX.IsPressed ? 1 : 0) + (negativeX.IsPressed ? -1 : 0),
            (positiveY.IsPressed ? 1 : 0) + (negativeY.IsPressed ? -1 : 0)
        ).Normalized();
    }

    public Vector2 GetVector(AnalogInput x, AnalogInput y)
    {
        if (Disabled)
            return Vector2.Zero;

        return new Vector2(GetAnalogValue(x), GetAnalogValue(y)).Normalized();
    }

    private void HandleDigitalPress(Keybind keybind, bool isPressed)
    {
        if (EventLogging)
            GD.Print($"{keybind.DisplayName} {(isPressed ? "pressed" : "released")}");

        if (isPressed && !keybind.IsPressed)
            keybind.Activate(true);
        else if (!isPressed && keybind.IsPressed)
            keybind.Activate(false);
    }

    private void HandleAnalogPress(Keybind keyBind, float value, float threshold)
    {
        HandleDigitalPress(keyBind, value > threshold);
    }

    /// <summary>
    /// Creates a dictionary resource from current state of keybinds
    /// </summary>
    private DictionaryResource CreateResourceFromValues()
    {
        DictionaryResource newResource = new();

        // Loop through keybinds and create a dictionary entry for each of them
        foreach (var keybind in GetKeybinds())
        {
            newResource.Dictionary.Add(
                keybind.DisplayName,
                new Godot.Collections.Array(keybind.BindedInputs.Select(input => (Variant)input))
            );
        }
        return newResource;
    }

    /// <summary>
    /// Loads keybinds from given resource dictionary
    /// </summary>
    public void LoadKeybinds(DictionaryResource resourceToLoad)
    {
        // Loop through keybinds and set value
        // if definition name is found in resource dictionary
        foreach (var keybind in GetKeybinds())
        {
            if (resourceToLoad.Dictionary.ContainsKey(keybind.DisplayName))
            {
                keybind.BindedInputs = resourceToLoad.Dictionary[keybind.DisplayName]
                    .As<Godot.Collections.Array>()
                    .Select(dict => new DigitalInput((Godot.Collections.Dictionary)dict))
                    .ToList();
            }
        }
        Defluo.Print("Loaded keybinds from resource");
    }

    /// <summary>
    /// Saves current keybindings to disk as a resource file
    /// </summary>
    public void SaveKeybinds()
    {
        keybindResource = CreateResourceFromValues();
        ResourceSaver.Save(keybindResource, UserKeybindsResourcePath);
        Defluo.Print("Saved keybinds to disk");
    }

    /// <summary>
    /// Resets all keybinds to their default bindings
    /// </summary>
    public void ResetKeybinds()
    {
        keybindResource = defaultKeybindResource;
        LoadKeybinds(keybindResource);
        KeybindsReset.Invoke();
        Defluo.Print("Reset keybinds to their default values");
    }

    /// <summary>
    /// Invoked when settings are reset
    /// </summary>
    public event Action KeybindsReset;

    public override void _ExitTree()
    {
        SaveKeybinds();
    }
}
