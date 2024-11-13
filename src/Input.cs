namespace DefluoLib;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Used to interface with mouse movement
/// </summary>
public class Mouse
{
    /// <summary>
    /// Threshold for when mouse is detected to be moved.
    /// </summary>
    public const float DEFAULT_VELOCITY_THRESHOLD = 10;

    /// <summary>
    /// Current position of mouse pointer in viewport
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Update position of mouse and run defined functions
    /// </summary>
    /// <param name="pos">Current position of mouse in viewport</param>
    /// <param name="relativePos">Position of mouse in relation to the last frame</param>
    internal void UpdatePosition(InputEventMouseMotion mouseMotionEvent)
    {
        Moved?.Invoke(mouseMotionEvent);
        Position = mouseMotionEvent.Position;
    }

    /// <summary>
    /// Invokes pressed and released events according to input event
    /// </summary>
    /// <param name="mouseButtonEvent"></param>
    internal void UpdateButton(InputEventMouseButton mouseButtonEvent)
    {
        if (mouseButtonEvent.Pressed)
            Pressed.Invoke(mouseButtonEvent.ButtonIndex);
        else
            Released.Invoke(mouseButtonEvent.ButtonIndex);
    }

    /// <summary>
    /// Sets the mouse position to specified vector.
    /// </summary>
    /// <param name="position"></param>
    public void Warp(Vector2 position) => Godot.Input.WarpMouse(position);

    /// <summary>
    /// Fired when mouse is moved
    /// </summary>
    public event Action<InputEventMouseMotion> Moved;

    /// <summary>
    /// Fired when mouse button is pressed down
    /// </summary>
    public event Action<MouseButton> Pressed;

    /// <summary>
    /// Fired when mouse button is released
    /// </summary>
    public event Action<MouseButton> Released;
}

/// <summary>
/// Node that handles access to input actions and updates their states
/// </summary>
public partial class Input : Node
{
    public const string USER_KEYBINDS_RESOURCE_PATH = "user://keybinds.tres";

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
        get => Godot.Input.MouseMode == Godot.Input.MouseModeEnum.Captured;
        set =>
            Godot.Input.MouseMode = value
                ? Godot.Input.MouseModeEnum.Captured
                : Godot.Input.MouseModeEnum.Visible;
    }

    private bool disabled;

    /// <summary>
    /// If input class is currently disabled,
    /// meaning non-essential actions are never triggered.
    /// Use this to pause the game and still keep essential keybinds functional,
    /// such as the pause key
    /// </summary>
    public bool Disabled
    {
        get => disabled;
        set
        {
            // When disabling input, reset press states
            if (value)
            {
                foreach (var keybind in keybinds)
                {
                    // Don't reset essential keybinds
                    if (!keybind.Options.IsEssential)
                        keybind.IsPressed = false;
                }
            }

            if (EventLogging)
                GD.Print(value ? "Input disabled" : "Input enabled");

            disabled = value;
        }
    }

    /// <summary>
    /// If true, a keybinding button is listening for input, meaning that next received input will be
    /// registered as a keybind and probably should not be used for anything else.
    /// </summary>
    /// <value></value>
    public bool RebindingButton { get; internal set; }

    /// <summary>
    /// If true, controller input was last used.
    /// If false, keyboard and mouse input was last used.
    /// </summary>
    public bool UsingControllerInput;

    /// <summary>
    /// Event that is invoked when input type changes between controller and keyboard.
    /// </summary>
    public event Action<bool> InputTypeChanged;

    /// <summary>
    /// Event that is invoked when a digital input is pressed.
    /// Action parameters: input that was pressed, pressed state of input (<c>true</c> = pressed)
    /// </summary>
    public event Action<DigitalInput, bool> DigitalInputPressed;

    /// <summary>
    /// Global UI accept action. Triggered by the keyboard buttons <c>Enter</c> and <c>Space</c> and the controller <c>A</c> button.
    /// </summary>
    [CreateCategory("UI")]
    public Keybind UIAccept { get; set; } =
        new Keybind(
            "UI Accept",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Enter,
            KeyboardInput.Space,
            ControllerDigitalInput.A
        );

    /// <summary>
    /// Global UI cancel action. Triggered by the keyboard buttons <c>Escape</c> and<c>Backspace</c> and the controller <c>B</c> button.
    /// </summary>
    public Keybind UICancel { get; set; } =
        new Keybind(
            "UI Cancel",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Escape,
            KeyboardInput.Backspace,
            ControllerDigitalInput.B
        );

    /// <summary>
    /// Global UI navigation action for moving left.
    /// </summary>
    /// <value></value>
    public Keybind UILeft { get; set; } =
        new Keybind(
            "UI Left",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Left,
            ControllerDigitalInput.DPadLeft,
            ControllerDigitalInput.LeftStickLeft
        );

    /// <summary>
    /// Global UI navigation action for moving right.
    /// </summary>
    /// <value></value>
    public Keybind UIRight { get; set; } =
        new Keybind(
            "UI Right",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Right,
            ControllerDigitalInput.DPadRight,
            ControllerDigitalInput.LeftStickRight
        );

    /// <summary>
    /// Global UI navigation action for moving up.
    /// </summary>
    /// <value></value>
    public Keybind UIUp { get; set; } =
        new Keybind(
            "UI Up",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Up,
            ControllerDigitalInput.DPadUp,
            ControllerDigitalInput.LeftStickUp
        );

    /// <summary>
    /// Global UI navigation action for moving down.
    /// </summary>
    /// <value></value>
    public Keybind UIDown { get; set; } =
        new Keybind(
            "UI Down",
            new KeybindOptions(isEssential: true, canBeRebinded: false),
            KeyboardInput.Down,
            ControllerDigitalInput.DPadDown,
            ControllerDigitalInput.LeftStickDown
        );

    /// <summary>
    /// Stores the previously stored pressed states of joy axis inputs.
    /// This is required to properly send pressed and released signals.
    /// </summary>
    public Dictionary<string, bool> JoyAxisLatestPressedStates = [];

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
        CreateCategoryAttribute? latestCategoryAttribute = null;
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

        if (ResourceLoader.Exists(USER_KEYBINDS_RESOURCE_PATH))
        {
            keybindResource = ResourceLoader.Load<DictionaryResource>(USER_KEYBINDS_RESOURCE_PATH);
            LoadKeybinds(keybindResource);
        }
        else
        {
            GD.Print("User keybinds not found, using default values");
            keybindResource = defaultKeybindResource;
        }
        SaveKeybinds();
    }

    public override void _Input(InputEvent @event)
    {
        if (InputLogging)
            GD.Print(@event.AsText());

        // Set controller input usage according to event type
        switch (@event)
        {
            case InputEventMouseMotion:
            case InputEventMouseButton:
            case InputEventKey:
                // Don't change input type if mouse movement velocity is below default threshold
                if (
                    @event is InputEventMouseMotion mouseEvent
                    && mouseEvent.Velocity.Length() < Mouse.DEFAULT_VELOCITY_THRESHOLD
                )
                {
                    break;
                }
                if (UsingControllerInput && InputTypeChanged != null)
                {
                    InputTypeChanged.Invoke(false);
                }
                UsingControllerInput = false;
                break;
            case InputEventJoypadMotion:
            case InputEventJoypadButton:
                // Don't change input type if axis input value is below default threshold
                if (
                    @event is InputEventJoypadMotion joypadEvent
                    && Mathf.Abs(joypadEvent.AxisValue) < Keybind.DEFAULT_AXIS_THRESHOLD
                )
                {
                    break;
                }
                if (!UsingControllerInput && InputTypeChanged != null)
                {
                    InputTypeChanged.Invoke(true);
                }
                UsingControllerInput = true;
                break;
        }

        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            // Update mouse class with the position and relative position vectors
            Mouse.UpdatePosition(mouseMotionEvent);
        }
        else
        {
            // Invoke mouse events when pressing mouse button
            if (@event is InputEventMouseButton mouseButtonEvent)
                Mouse.UpdateButton(mouseButtonEvent);

            // Loop through keybinds
            foreach (var keybind in keybinds)
            {
                // If input is disabled, only run actions for essential keybinds
                if (Disabled && !keybind.Options.IsEssential)
                    continue;

                // See if event key has been binded to an action
                foreach (var bindedInput in keybind.BindedInputs)
                {
                    var pressed = bindedInput.ParseValue(@event);
                    if (pressed.HasValue)
                    {
                        ActivateKeybind(keybind, pressed.Value);
                        break;
                    }
                }
            }

            // Parse DigitalInput from input event
            var input = DigitalInput.FromEvent(@event);
            // Invoke input event if input could be parsed
            // ParseValue should always return a boolean since the event applies to input
            if (DigitalInputPressed != null && input != null)
            {
#pragma warning disable CS8629
                var value = input.ParseValue(@event).Value;
#pragma warning restore CS8629
                // If event is an analog event, invoke event only if pressed state has changed

                if (@event is InputEventJoypadMotion)
                {
                    JoyAxisLatestPressedStates.TryAdd(input.DisplayString, value);
                    if (JoyAxisLatestPressedStates[input.DisplayString] != value)
                    {
                        GD.Print(value);
                        DigitalInputPressed.Invoke(input, value);
                        JoyAxisLatestPressedStates[input.DisplayString] = value;
                    }
                }
                else
                    DigitalInputPressed.Invoke(input, value);
            }
        }
    }

    public override void _Process(double delta)
    {
        // Update process frame versions of just pressed and just released variables
        // of all registered keybinds
        foreach (var keybind in keybinds)
        {
            if (keybind.IsPressed)
            {
                keybind.processIsJustPressed = keybind.ProcessFramesHeldDown == 0;
                keybind.ProcessFramesHeldDown++;
            }
            else
            {
                keybind.processIsJustReleased = keybind.ProcessFramesHeldDown != 0;
                keybind.ProcessFramesHeldDown = 0;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Update physics frame versions of just pressed and just released variables
        // of all registered keybinds
        foreach (var keybind in keybinds)
        {
            if (keybind.IsPressed)
            {
                keybind.physicsIsJustPressed = keybind.PhysicsFramesHeldDown == 0;
                keybind.PhysicsFramesHeldDown++;
            }
            else
            {
                keybind.physicsIsJustReleased = keybind.PhysicsFramesHeldDown != 0;
                keybind.PhysicsFramesHeldDown = 0;
            }
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
    /// Returns a list of defined keybind properties
    /// </summary>
    /// <returns></returns>
    internal static List<PropertyInfo> GetKeybindProperties()
    {
        var list =
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
#pragma warning disable CS8600
        var list = properties
            .Select(property => (Keybind)property.GetValue(this))
            .ToList();
#pragma warning restore CS8600
        return [.. list];
    }

    /// <summary>
    /// Waits for a digital input to be pressed, and returns it
    /// </summary>
    /// <param name="pressed">
    /// Whether or not digital input should be pressed down.
    /// If unspecified, will return input regardless of its pressed state.
    /// </param>
    public Task<DigitalInput> AwaitForDigitalInput(bool? pressed = null)
    {
        var result = new TaskCompletionSource<DigitalInput>();
        void eventHandler(DigitalInput input, bool inputPressed)
        {
            if (pressed.HasValue)
            {
                if (inputPressed == pressed)
                {
                    result.SetResult(input);
                    DigitalInputPressed -= eventHandler;
                }
            }
            else
            {
                result.SetResult(input);
                DigitalInputPressed -= eventHandler;
            }
        }
        DigitalInputPressed += eventHandler;
        return result.Task;
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
        float axisThreshold = Keybind.DEFAULT_AXIS_THRESHOLD
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

        return new Vector2(GetAnalogValue(x), GetAnalogValue(y));
    }

    private void ActivateKeybind(Keybind keybind, bool isPressed)
    {
        if (isPressed && !keybind.IsPressed)
        {
            if (EventLogging)
                GD.Print($"{keybind.DisplayName} pressed");
            keybind.Activate(true);
        }
        else if (!isPressed && keybind.IsPressed)
        {
            if (EventLogging)
                GD.Print($"{keybind.DisplayName} released");
            keybind.Activate(false);
        }
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
            // Don't create entries for non-rebindable keybinds
            if (keybind.Options.CanBeRebinded)
                newResource.Dictionary.Add(
                    keybind.DisplayName,
                    new Godot.Collections.Array(
                        keybind.BindedInputs.Select(input => (Variant)input)
                    )
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
            if (resourceToLoad.Dictionary.TryGetValue(keybind.DisplayName, out var value))
            {
                keybind.BindedInputs = value.As<Godot.Collections.Array>()
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
        ResourceSaver.Save(keybindResource, USER_KEYBINDS_RESOURCE_PATH);
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
