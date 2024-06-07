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

    public List<TextureButton> Buttons = new();

    public void Initialize(Keybind keybind)
    {
        Keybind = keybind;
        GetNode<Label>(NameLabel).Text = keybind.DisplayName;

        // Add buttons to list only if previous button has also been added
        Buttons = new();
        if (!Button1.IsEmpty)
        {
            Buttons.Add(GetNode<TextureButton>(Button1));
            if (!Button2.IsEmpty)
            {
                Buttons.Add(GetNode<TextureButton>(Button2));
                if (!Button3.IsEmpty)
                {
                    Buttons.Add(GetNode<TextureButton>(Button3));
                    if (!Button4.IsEmpty)
                    {
                        Buttons.Add(GetNode<TextureButton>(Button4));
                    }
                }
            }
        }

        foreach (var (button, index) in Buttons.WithIndex())
        {
            if (keybind.BindedInputs.Count > index)
                button.TextureNormal = Input.GetInputTexture(keybind.BindedInputs[index]);
            else
                button.TextureNormal = null;

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
