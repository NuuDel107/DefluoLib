using Godot;
using System.Collections.Generic;

namespace DefluoLib;

[GlobalClass]
internal partial class KeybindTemplate : HBoxContainer
{
    [Export]
    public NodePath NameLabel;

    [ExportGroup("Keybind Buttons")]
    [Export]
    public NodePath Button1;

    [Export]
    public NodePath Button2;

    [Export]
    public NodePath Button3;
    [Export]
    public NodePath Button4;

    public Keybind Keybind;
    public DigitalInput[] KeyboardInputs;
    public DigitalInput[] ControllerInputs;

    public List<Button> Buttons;

    public void Initialize(Keybind keybind)
    {
        Keybind = keybind;
        GetNode<Label>(NameLabel).Text = keybind.DisplayName;

        // Add buttons to list only if previous button has also been added
        Buttons = new();
        if (!Button1.IsEmpty)
        {
            Buttons.Add(GetNode<Button>(Button1));
            if (!Button2.IsEmpty)
            {
                Buttons.Add(GetNode<Button>(Button2));
                if (!Button3.IsEmpty)
                {
                    Buttons.Add(GetNode<Button>(Button3));
                    if (!Button4.IsEmpty)
                    {
                        Buttons.Add(GetNode<Button>(Button4));
                    }
                }
            }
        }

        // Sort keyboard inputs and controllerinputs into their separate arrays
        KeyboardInputs = new DigitalInput[Buttons.Count];
        ControllerInputs = new DigitalInput[Buttons.Count];

        var k = 0;
        var c = 0;
        foreach (var input in Keybind.BindedInputs)
        {
            if (input.Type == DigitalInputType.Key || input.Type == DigitalInputType.MouseButton)
            {
                KeyboardInputs[k] = input;
                k++;
            }
            else
            {
                ControllerInputs[c] = input;
                c++;
            }
        }

        UpdateIcons(Defluo.Input.UsingControllerInput);
        Defluo.Input.InputTypeChanged += UpdateIcons;

        foreach (var (button, index) in Buttons.WithIndex())
        {
            button.Pressed += async () =>
            {
                // Set rebinding state to true so menu UI code
                // knows to ignore UICancel keybind
                Defluo.Input.RebindingButton = true;

                // Lock button for input picking
                button.Icon = null;
                button.FocusMode = FocusModeEnum.None;
                button.Disabled = true;

                // Save next digital input as keybind
                var input = await Defluo.Input.AwaitForDigitalInput(pressed: false);

                // Change keybind on currently active input type
                var controllerInput = Defluo.Input.UsingControllerInput;
                if (controllerInput)
                    ControllerInputs[index] = input;
                else
                    KeyboardInputs[index] = input;

                // Release button
                button.Disabled = false;
                button.FocusMode = FocusModeEnum.All;
                button.GrabFocus();

                // Update keybind list and icons
                UpdateInputs();
                UpdateIcons(Defluo.Input.UsingControllerInput);

                // If cancel button is pressed after assignment, input should be unbinded
                var cancelInput = await Defluo.Input.AwaitForDigitalInput(pressed: true);
                foreach (var bindedInput in Defluo.Input.UICancel.BindedInputs)
                {
                    if (bindedInput.Equals(cancelInput))
                    {
                        if (controllerInput)
                            ControllerInputs[index] = null;
                        else
                            KeyboardInputs[index] = null;

                        UpdateInputs();
                        UpdateIcons(Defluo.Input.UsingControllerInput);
                        break;
                    }
                }

                // Wait for two frames before updating rebinding state so UI code can ignore input
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Defluo.Input.RebindingButton = false;
            };
        }

    }

    /// <summary>
    /// Updates the input list on associated keybind
    /// </summary>
    private void UpdateInputs()
    {
        Keybind.BindedInputs = new();
        foreach (var input in ControllerInputs)
        {
            if (input != null)
                Keybind.BindedInputs.Add(input);
        }
        foreach (var input in KeyboardInputs)
        {
            if (input != null)
                Keybind.BindedInputs.Add(input);
        }
    }

    /// <summary>
    /// Updates the associated UI button icons depending on current input type
    /// </summary>
    /// <param name="controllerInput"></param>
    private void UpdateIcons(bool controllerInput)
    {
        foreach (var (input, index) in (controllerInput ? ControllerInputs : KeyboardInputs).WithIndex())
        {
            if (input == null || Buttons[index].Disabled)
                Buttons[index].Icon = null;
            else
                Buttons[index].Icon = Input.GetInputTexture(input);
        }
    }
}
