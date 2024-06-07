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

    private Keybind Keybind;

    public List<Button> Buttons = new();

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

        foreach (var (button, index) in Buttons.WithIndex())
        {
            if (keybind.BindedInputs.Count > index)
                button.Icon = Input.GetInputTexture(keybind.BindedInputs[index]);
            else
                button.Icon = null;

            button.Pressed += async () =>
            {
                button.Icon = null;
                button.FocusMode = FocusModeEnum.None;
                // Await for UI input to release
                await Defluo.Input.AwaitForDigitalInput(pressed: false);
                // Save next digital input as keybind
                var input = await Defluo.Input.AwaitForDigitalInput(pressed: false);
                button.FocusMode = FocusModeEnum.All;
                button.GrabFocus();
                Keybind.BindedInputs[index] = input;
                button.Icon = Input.GetInputTexture(input);
            };
        }
    }
}
