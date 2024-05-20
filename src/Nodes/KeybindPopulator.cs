using Godot;

namespace DefluoLib;

[GlobalClass]
internal partial class KeybindPopulator : Control
{
    [Export]
    public Container Container;

    [Export]
    public KeybindTemplate Template;

    [Export]
    public CategoryDivider Divider;

    public override void _Ready()
    {
        Template.GetParent().RemoveChild(Template);
        Divider?.GetParent().RemoveChild(Divider);

        var latestCategory = "";
        foreach (var keybind in Defluo.Input.GetKeybinds())
        {
            if (Divider != null)
            {
                if (keybind.Category != latestCategory)
                {
                    latestCategory = keybind.Category;
                    var divider = (CategoryDivider)Divider.Duplicate();
                    divider.Initialize(latestCategory);
                    Container.AddChild(divider);
                }
            }
            var template = (KeybindTemplate)Template.Duplicate();
            template.Name = keybind.DisplayName;
            Container.AddChild(template);
            template.Initialize(keybind);
        }
    }
}
