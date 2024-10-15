using System;
using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// Enum representing the possible source types for a digital input
/// </summary>
public enum DigitalInputType
{
    /// <summary>
    /// Key on a keyboard
    /// </summary>
    Key,

    /// <summary>
    /// Button on a mouse
    /// </summary>
    MouseButton,

    /// <summary>
    /// Button on a controller
    /// </summary>
    JoyButton,

    /// <summary>
    /// Analog trigger or joystick on a controller
    /// </summary>
    JoyAxis
}

public class DigitalInput
{
    private readonly Key Key;
    private readonly MouseButton MouseButton;
    private readonly JoyButton JoyButton;
    private readonly JoyAxis JoyAxis;

    /// <summary>
    /// Type of digital input
    /// </summary>
    public readonly DigitalInputType Type;

    /// <summary>
    /// If this digital input represents a analog event with a positive value
    /// </summary>
    public readonly bool PositiveAnalog = true;

    /// <summary>
    /// Display string for the input. Translated with keys from <c>locale/input.pot</c>
    /// </summary>
    public string DisplayString
    {
        get
        {
            string buttonName = Type switch
            {
                DigitalInputType.Key => ((Key)EnumValue).ToString(),
                DigitalInputType.MouseButton => ((MouseButton)EnumValue).ToString(),
                DigitalInputType.JoyButton => ((JoyButton)EnumValue).ToString(),
                DigitalInputType.JoyAxis => ((JoyAxis)EnumValue).ToString(),
                _ => throw new NotImplementedException()
            };
            string buttonType = Type switch
            {
                DigitalInputType.Key => "KEY",
                DigitalInputType.MouseButton => "MOUSE",
                DigitalInputType.JoyButton or DigitalInputType.JoyAxis => "JOYDIGITAL",
                _ => throw new NotImplementedException()
            };
            string buttonAnalogType = "";
            if (Type == DigitalInputType.JoyAxis)
            {
                buttonAnalogType = PositiveAnalog ? "_POSITIVE" : "_NEGATIVE";
            }
            return Defluo.Tr($"INPUT_{buttonType}_{buttonName.ToUpper()}{buttonAnalogType}");
        }
    }

    /// <summary>
    /// Raw value from the Godot input enum that this input represents
    /// </summary>
    public long EnumValue
    {
        get
        {
            return Type switch
            {
                DigitalInputType.Key => (long)Key,
                DigitalInputType.MouseButton => (long)MouseButton,
                DigitalInputType.JoyButton => (long)JoyButton,
                DigitalInputType.JoyAxis => (long)JoyAxis,
                _ => throw new NotImplementedException()
            };
        }
    }

    public DigitalInput(Key key)
    {
        Key = key;
        Type = DigitalInputType.Key;
    }

    public DigitalInput(MouseButton mouseButton)
    {
        MouseButton = mouseButton;
        Type = DigitalInputType.MouseButton;
    }

    public DigitalInput(JoyButton joyButton)
    {
        JoyButton = joyButton;
        Type = DigitalInputType.JoyButton;
    }

    public DigitalInput(JoyAxis joyAxis, bool positive)
    {
        JoyAxis = joyAxis;
        PositiveAnalog = positive;
        Type = DigitalInputType.JoyAxis;
    }

    public DigitalInput(Dictionary dictionary)
    {
        var parameters = new Variant[3];
        if (
            dictionary.TryGetValue("type", out parameters[0])
            && Enum.TryParse((string)parameters[0], out Type)
            && dictionary.TryGetValue("enum", out parameters[1])
            && dictionary.TryGetValue("positiveAnalog", out parameters[2])
        )
        {
            PositiveAnalog = (bool)parameters[2];

            switch (Type)
            {
                case DigitalInputType.Key:
                    Key = (Key)(long)parameters[1];
                    break;
                case DigitalInputType.MouseButton:
                    MouseButton = (MouseButton)(long)parameters[1];
                    break;
                case DigitalInputType.JoyButton:
                    JoyButton = (JoyButton)(long)parameters[1];
                    break;
                case DigitalInputType.JoyAxis:
                    JoyAxis = (JoyAxis)(long)parameters[1];
                    break;
            }
        }
        else
            throw new ArgumentException("Dictionary not valid for initialization");
    }

    public static explicit operator Dictionary(DigitalInput input)
    {
        return new()
        {
            { "type", input.Type.ToString() },
            { "enum", input.EnumValue },
            { "positiveAnalog", input.PositiveAnalog }
        };
    }

    public static explicit operator Variant(DigitalInput input) => (Dictionary)input;

    public bool Equals(Key key) => Type == DigitalInputType.Key && (Key)EnumValue == key;

    public bool Equals(MouseButton mouseButton) =>
        Type == DigitalInputType.MouseButton && (MouseButton)EnumValue == mouseButton;

    public bool Equals(JoyButton joyButton) =>
        Type == DigitalInputType.JoyButton && (JoyButton)EnumValue == joyButton;

    public bool Equals(JoyAxis joyAxis) =>
        Type == DigitalInputType.JoyAxis && (JoyAxis)EnumValue == joyAxis;

    public bool Equals(JoyAxis joyAxis, bool positiveAnalog) =>
        Type == DigitalInputType.JoyAxis
        && (JoyAxis)EnumValue == joyAxis
        && positiveAnalog == PositiveAnalog;

    public bool Equals(DigitalInput input)
    {
        if (input.Type == Type)
            switch (Type)
            {
                case DigitalInputType.Key:
                    return Equals(input.Key);
                case DigitalInputType.MouseButton:
                    return Equals(input.MouseButton);
                case DigitalInputType.JoyButton:
                    return Equals(input.JoyButton);
                case DigitalInputType.JoyAxis:
                    return Equals(input.JoyAxis, input.PositiveAnalog);
            }
        return false;
    }

    /// <summary>
    /// Parses the pressed state from event, if event applies to digital input
    /// </summary>
    /// <returns>Pressed state of digital input in event, or null if event doesn't apply</returns>
    public bool? ParseValue(InputEvent @event, float axisThreshold = Keybind.DefaultAxisThreshold)
    {
        if (
            @event is InputEventKey keyEvent
            // If physical keycode is empty, check for equality using the regular keycode instead
            && Equals(
                keyEvent.PhysicalKeycode == Key.None ? keyEvent.Keycode : keyEvent.PhysicalKeycode
            )
        )
            return keyEvent.Pressed;
        else if (
            @event is InputEventMouseButton mouseButtonEvent && Equals(mouseButtonEvent.ButtonIndex)
        )
            return mouseButtonEvent.Pressed;
        else if (@event is InputEventJoypadButton joypadEvent && Equals(joypadEvent.ButtonIndex))
            return joypadEvent.Pressed;
        else if (
            @event is InputEventJoypadMotion joypadMotionEvent
            && Equals(joypadMotionEvent.Axis, joypadMotionEvent.AxisValue > 0)
        )
        {
            if (Mathf.Abs(joypadMotionEvent.AxisValue) >= axisThreshold)
                return true;
            else if (Mathf.Abs(joypadMotionEvent.AxisValue) < axisThreshold)
                return false;
            else
                return null;
        }
        else
            return null;
    }

    /// <summary>
    /// Returns a <c>DigitalInput</c> class based on the button that caused the event.
    /// Returns <c>null</c> if class couldn't be parsed from the event
    /// </summary>
    /// <param name="event">Event to parse DigitalInput from</param>
    public static DigitalInput FromEvent(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            return new DigitalInput(keyEvent.PhysicalKeycode);
        }
        else if (@event is InputEventMouseButton mouseButtonEvent)
        {
            return new DigitalInput(mouseButtonEvent.ButtonIndex);
        }
        else if (@event is InputEventJoypadButton joypadEvent)
        {
            return new DigitalInput(joypadEvent.ButtonIndex);
        }
        else if (@event is InputEventJoypadMotion joypadMotionEvent)
        {
            return new DigitalInput(joypadMotionEvent.Axis, joypadMotionEvent.AxisValue > 0);
        }
        else
            return null;
    }
}

public class AnalogInput
{
    public JoyAxis Axis;
    public string DisplayName;

    public AnalogInput(JoyAxis joyAxis)
    {
        Axis = joyAxis;
    }
}

public static class MouseInput
{
    /// <summary>
    /// Primary (left) mouse button
    /// </summary>
    public static readonly DigitalInput LeftButton = new(MouseButton.Left);

    /// <summary>
    /// Secondary (right) mouse button
    /// </summary>
    public static readonly DigitalInput RightButton = new(MouseButton.Right);

    /// <summary>
    /// Middle mouse button / wheel pressed down
    /// </summary>
    public static readonly DigitalInput MiddleButton = new(MouseButton.Middle);

    /// <summary>
    /// Wheel scrolled up
    /// </summary>
    public static readonly DigitalInput WheelUp = new(MouseButton.WheelUp);

    /// <summary>
    /// Wheel scrolled down
    /// </summary>
    public static readonly DigitalInput WheelDown = new(MouseButton.WheelDown);

    /// <summary>
    /// Wheel left button
    /// </summary>
    public static readonly DigitalInput WheelLeftButton = new(MouseButton.WheelLeft);

    /// <summary>
    /// Wheel right button
    /// </summary>
    public static readonly DigitalInput WheelRightButton = new(MouseButton.WheelRight);

    /// <summary>
    /// Extra (side) button 1
    /// </summary>
    public static readonly DigitalInput ExtraButton1 = new(MouseButton.Xbutton1);

    /// <summary>
    /// Extra (side) button 2
    /// </summary>
    public static readonly DigitalInput ExtraButton2 = new(MouseButton.Xbutton2);
}

public static class ControllerDigitalInput
{
    /// <summary>
    /// Xbox: A, PS: Cross, Nintendo: B
    /// </summary>
    public static readonly DigitalInput A = new(JoyButton.A);

    /// <summary>
    /// Xbox: B, PS: Circle, Nintendo: A
    /// </summary>
    public static readonly DigitalInput B = new(JoyButton.B);

    /// <summary>
    /// Xbox: X, PS: Square, Nintendo: Y
    /// </summary>
    public static readonly DigitalInput X = new(JoyButton.X);

    /// <summary>
    /// Xbox: Y, PS: Triangle, Nintendo: X
    /// </summary>
    public static readonly DigitalInput Y = new(JoyButton.Y);

    /// <summary>
    /// D-pad Up button
    /// </summary>
    public static readonly DigitalInput DPadUp = new(JoyButton.DpadUp);

    /// <summary>
    /// D-pad Down button
    /// </summary>
    public static readonly DigitalInput DPadDown = new(JoyButton.DpadDown);

    /// <summary>
    /// D-pad Left button
    /// </summary>
    public static readonly DigitalInput DPadLeft = new(JoyButton.DpadLeft);

    /// <summary>
    /// D-pad Right button
    /// </summary>
    public static readonly DigitalInput DPadRight = new(JoyButton.DpadRight);

    /// <summary>
    /// Left joystick moved up
    /// </summary>
    public static readonly DigitalInput LeftStickUp = new(JoyAxis.LeftY, false);

    /// <summary>
    /// Left joystick moved down
    /// </summary>
    public static readonly DigitalInput LeftStickDown = new(JoyAxis.LeftY, true);

    /// <summary>
    /// Left joystick moved left
    /// </summary>
    public static readonly DigitalInput LeftStickLeft = new(JoyAxis.LeftX, false);

    /// <summary>
    /// Left joystick moved right
    /// </summary>
    public static readonly DigitalInput LeftStickRight = new(JoyAxis.LeftX, true);

    /// <summary>
    /// Right joystick moved up
    /// </summary>
    public static readonly DigitalInput RightStickUp = new(JoyAxis.RightY, false);

    /// <summary>
    /// Right joystick moved down
    /// </summary>
    public static readonly DigitalInput RightStickDown = new(JoyAxis.RightY, true);

    /// <summary>
    /// Right joystick moved left
    /// </summary>
    public static readonly DigitalInput RightStickLeft = new(JoyAxis.RightX, false);

    /// <summary>
    /// Right joystick moved right
    /// </summary>
    public static readonly DigitalInput RightStickRight = new(JoyAxis.RightX, true);

    /// <summary>
    /// Left shoulder button (Xbox: LB, PS: L1)
    /// </summary>
    public static readonly DigitalInput LeftShoulder = new(JoyButton.LeftShoulder);

    /// <summary>
    /// Right shoulder button (Xbox: RB, PS: R1)
    /// </summary>
    public static readonly DigitalInput RightShoulder = new(JoyButton.RightShoulder);

    /// <summary>
    /// Left trigger (Xbox: LT, PS: L2)
    /// </summary>
    public static readonly DigitalInput LeftTrigger = new(JoyAxis.TriggerLeft, true);

    /// <summary>
    /// Right trigger (Xbox: RT, PS: R2)
    /// </summary>
    public static readonly DigitalInput RightTrigger = new(JoyAxis.TriggerRight, true);

    /// <summary>
    /// Left joystick pressed down (Xbox: LS, PS: L3)
    /// </summary>
    public static readonly DigitalInput LeftStickPress = new(JoyButton.LeftStick);

    /// <summary>
    /// Right joystick pressed down (Xbox: RS, PS: R3)
    /// </summary>
    public static readonly DigitalInput RightStickPress = new(JoyButton.RightStick);

    /// <summary>
    /// Xbox: Menu, PS: Options, Nintendo: +
    /// </summary>
    public static readonly DigitalInput Start = new(JoyButton.Start);

    /// <summary>
    /// Xbox: Back, PS: Share, Nintendo: -
    /// </summary>
    public static readonly DigitalInput Select = new(JoyButton.Back);

    /// <summary>
    /// Xbox: Home, PS: PS button
    /// </summary>
    public static readonly DigitalInput Home = new(JoyButton.Guide);

    /// <summary>
    /// Touchpad button (PS)
    /// </summary>
    public static readonly DigitalInput Touchpad = new(JoyButton.Touchpad);

    /// <summary>
    /// Xbox: Share, PS: Microphone, Nintendo: Capture
    /// </summary>
    public static readonly DigitalInput Misc = new(JoyButton.Misc1);
}

public static class ControllerAnalogInput
{
    /// <summary>
    /// Left joystick X axis
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>-1.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput LeftStickX = new(JoyAxis.LeftX);

    /// <summary>
    /// Left joystick Y axis
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>-1.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput LeftStickY = new(JoyAxis.LeftY);

    /// <summary>
    /// Right joystick X axis
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>-1.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput RightStickX = new(JoyAxis.RightX);

    /// <summary>
    /// Right joystick Y axis
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>-1.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput RightStickY = new(JoyAxis.RightX);

    /// <summary>
    /// Left trigger
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>0.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput LeftTrigger = new(JoyAxis.TriggerLeft);

    /// <summary>
    /// Right trigger
    /// </summary>
    /// <remarks>
    /// Range of analog values: <c>0.0</c> to <c>1.0</c>
    /// </remarks>
    public static readonly AnalogInput RightTrigger = new(JoyAxis.TriggerRight);
}

public static class KeyboardInput
{
    public static readonly DigitalInput A = new(Key.A);
    public static readonly DigitalInput B = new(Key.B);
    public static readonly DigitalInput C = new(Key.C);
    public static readonly DigitalInput D = new(Key.D);
    public static readonly DigitalInput E = new(Key.E);
    public static readonly DigitalInput F = new(Key.F);
    public static readonly DigitalInput G = new(Key.G);
    public static readonly DigitalInput H = new(Key.H);
    public static readonly DigitalInput I = new(Key.I);
    public static readonly DigitalInput J = new(Key.J);
    public static readonly DigitalInput K = new(Key.K);
    public static readonly DigitalInput L = new(Key.L);
    public static readonly DigitalInput M = new(Key.M);
    public static readonly DigitalInput N = new(Key.N);
    public static readonly DigitalInput O = new(Key.O);
    public static readonly DigitalInput P = new(Key.P);
    public static readonly DigitalInput Q = new(Key.Q);
    public static readonly DigitalInput R = new(Key.R);
    public static readonly DigitalInput S = new(Key.S);
    public static readonly DigitalInput T = new(Key.T);
    public static readonly DigitalInput U = new(Key.U);
    public static readonly DigitalInput V = new(Key.V);
    public static readonly DigitalInput W = new(Key.W);
    public static readonly DigitalInput X = new(Key.X);
    public static readonly DigitalInput Y = new(Key.Y);
    public static readonly DigitalInput Z = new(Key.Z);
    public static readonly DigitalInput Left = new(Key.Left);
    public static readonly DigitalInput Right = new(Key.Right);
    public static readonly DigitalInput Up = new(Key.Up);
    public static readonly DigitalInput Down = new(Key.Down);
    public static readonly DigitalInput Space = new(Key.Space);
    public static readonly DigitalInput Enter = new(Key.Enter);
    public static readonly DigitalInput Escape = new(Key.Escape);
    public static readonly DigitalInput Ctrl = new(Key.Ctrl);
    public static readonly DigitalInput Shift = new(Key.Shift);
    public static readonly DigitalInput Alt = new(Key.Alt);
    public static readonly DigitalInput Insert = new(Key.Insert);
    public static readonly DigitalInput Home = new(Key.Home);
    public static readonly DigitalInput Delete = new(Key.Delete);
    public static readonly DigitalInput End = new(Key.End);
    public static readonly DigitalInput PageUp = new(Key.Pageup);
    public static readonly DigitalInput PageDown = new(Key.Pagedown);
    public static readonly DigitalInput PrintScreen = new(Key.Print);
    public static readonly DigitalInput CapsLock = new(Key.Capslock);
    public static readonly DigitalInput Tab = new(Key.Tab);
    public static readonly DigitalInput Backspace = new(Key.Backspace);
    public static readonly DigitalInput Key0 = new(Key.Key0);
    public static readonly DigitalInput Key1 = new(Key.Key1);
    public static readonly DigitalInput Key2 = new(Key.Key2);
    public static readonly DigitalInput Key3 = new(Key.Key3);
    public static readonly DigitalInput Key4 = new(Key.Key4);
    public static readonly DigitalInput Key5 = new(Key.Key5);
    public static readonly DigitalInput Key6 = new(Key.Key6);
    public static readonly DigitalInput Key7 = new(Key.Key7);
    public static readonly DigitalInput Key8 = new(Key.Key8);
    public static readonly DigitalInput Key9 = new(Key.Key9);
    public static readonly DigitalInput Keypad0 = new(Key.Kp0);
    public static readonly DigitalInput Keypad1 = new(Key.Kp1);
    public static readonly DigitalInput Keypad2 = new(Key.Kp2);
    public static readonly DigitalInput Keypad3 = new(Key.Kp3);
    public static readonly DigitalInput Keypad4 = new(Key.Kp4);
    public static readonly DigitalInput Keypad5 = new(Key.Kp5);
    public static readonly DigitalInput Keypad6 = new(Key.Kp6);
    public static readonly DigitalInput Keypad7 = new(Key.Kp7);
    public static readonly DigitalInput Keypad8 = new(Key.Kp8);
    public static readonly DigitalInput Keypad9 = new(Key.Kp9);
    public static readonly DigitalInput KeypadPeriod = new(Key.KpPeriod);
    public static readonly DigitalInput NumLock = new(Key.Numlock);
    public static readonly DigitalInput KeypadDivide = new(Key.KpDivide);
    public static readonly DigitalInput KeypadMultiply = new(Key.KpMultiply);
    public static readonly DigitalInput KeypadSubtract = new(Key.KpSubtract);
    public static readonly DigitalInput KeypadAdd = new(Key.KpAdd);
    public static readonly DigitalInput KeypadEnter = new(Key.KpEnter);
    public static readonly DigitalInput F1 = new(Key.F1);
    public static readonly DigitalInput F2 = new(Key.F2);
    public static readonly DigitalInput F3 = new(Key.F3);
    public static readonly DigitalInput F4 = new(Key.F4);
    public static readonly DigitalInput F5 = new(Key.F5);
    public static readonly DigitalInput F6 = new(Key.F6);
    public static readonly DigitalInput F7 = new(Key.F7);
    public static readonly DigitalInput F8 = new(Key.F8);
    public static readonly DigitalInput F9 = new(Key.F9);
    public static readonly DigitalInput F10 = new(Key.F10);
    public static readonly DigitalInput F11 = new(Key.F11);
    public static readonly DigitalInput F12 = new(Key.F12);
    public static readonly DigitalInput F13 = new(Key.F13);
    public static readonly DigitalInput F14 = new(Key.F14);
    public static readonly DigitalInput F15 = new(Key.F15);
    public static readonly DigitalInput F16 = new(Key.F16);
    public static readonly DigitalInput F17 = new(Key.F17);
    public static readonly DigitalInput F18 = new(Key.F18);
    public static readonly DigitalInput F19 = new(Key.F19);
    public static readonly DigitalInput F20 = new(Key.F20);
    public static readonly DigitalInput F21 = new(Key.F21);
    public static readonly DigitalInput F22 = new(Key.F22);
    public static readonly DigitalInput F23 = new(Key.F23);
    public static readonly DigitalInput F24 = new(Key.F24);
}
