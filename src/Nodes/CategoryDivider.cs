using Godot;

namespace DefluoLib;

[GlobalClass]
internal partial class CategoryDivider : HBoxContainer
{
    [Export]
    public NodePath Label;

    public void Initialize(string categoryName)
    {
        GetNode<Label>(Label).Text = categoryName;
    }
}
