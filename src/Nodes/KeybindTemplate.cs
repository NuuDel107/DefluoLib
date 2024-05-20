using Godot;
using System.Collections.Generic;

namespace DefluoLib;

[Tool]
[GlobalClass]
internal partial class KeybindTemplate : HBoxContainer
{
    [Export]
    public Label NameLabel;

    [ExportGroup("Keybind Buttons")]
    [Export]
    public TextureButton Button1;

    [Export]
    public TextureButton Button2;

    [Export]
    public TextureButton Button3;

    [Export]
    public TextureButton Button4;

    private Keybind Keybind;

    public List<TextureButton> Buttons = new();

    public void Initialize(Keybind keybind)
    {
        Keybind = keybind;
        NameLabel.Text = keybind.DisplayName;

        // Add buttons to list only if previous button has also been added
        if (Button1 != null)
        {
            Buttons.Add(Button1);
            if (Button2 != null)
            {
                Buttons.Add(Button2);
                if (Button3 != null)
                {
                    Buttons.Add(Button3);
                    if (Button4 != null)
                    {
                        Buttons.Add(Button4);
                    }
                }
            }
        }

        foreach (var (button, index) in Buttons.WithIndex())
        {
            if (keybind.BindedInputs.Count > index)
                button.TextureNormal = Input.GetInputTexture(keybind.BindedInputs[index]);

            button.Pressed += async () =>
            {
                button.TextureNormal = null;
                var input = await Defluo.Input.AwaitForDigitalInput();
                GD.Print(input.DisplayString);
                Keybind.BindedInputs[index] = input;
                button.TextureNormal = Input.GetInputTexture(input);
            };
        }
    }
}
