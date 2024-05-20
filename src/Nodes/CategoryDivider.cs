using Godot;

namespace DefluoLib;

[GlobalClass]
internal partial class CategoryDivider : HBoxContainer
{
    [Export]
    public Label Label;

    public void Initialize(string categoryName)
    {
        Label.Text = categoryName;
    }
}
