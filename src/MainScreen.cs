using Godot;
using Godot.Collections;

namespace DefluoLib;

[Tool]
public partial class MainScreen : Control
{
    [Export]
    public Array<Texture2D> MainTabIcons;

    [Export]
    public Array<Texture2D> FMODTabIcons;

    [Export]
    public TabContainer TabContainer;

    [Export]
    public TabContainer FMODTabContainer;

    [Export]
    public Tree EventTree;

    [Export]
    public Tree BusTree;

    [Export]
    public Tree VCATree;

    [Export]
    public Tree ParameterTree;

    [Export]
    public Button RefreshFMODButton;

    public override void _Ready()
    {
        foreach (var (icon, index) in MainTabIcons.WithIndex())
        {
            TabContainer?.SetTabIcon(index, icon);
        }
        foreach (var (icon, index) in FMODTabIcons.WithIndex())
        {
            FMODTabContainer?.SetTabIcon(index, icon);
        }

        InitTrees();
        RefreshFMODButton.Pressed += Refresh;
    }

    private void InitTrees()
    {
        EventTree.Clear();
        FMODEditorProperty.InitEventTree(EventTree);
        BusTree.Clear();
        FMODEditorProperty.InitBusTree(BusTree);
        VCATree.Clear();
        FMODEditorProperty.InitVCATree(VCATree);
        ParameterTree.Clear();
        FMODEditorProperty.InitParameterTree(ParameterTree);
    }

    private void Refresh()
    {
        DefluoLib.Singleton.FMODLister.Refresh();
        InitTrees();
    }
}
