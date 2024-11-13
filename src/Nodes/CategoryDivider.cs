namespace DefluoLib;

using Godot;

[GlobalClass]
public partial class CategoryDivider : HBoxContainer
{
    [Export]
    public NodePath Label;

    public void Initialize(string categoryName)
    {
        GetNode<Label>(Label).Text = categoryName;
    }
}
