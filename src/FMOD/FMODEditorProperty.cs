namespace DefluoLib;

using Godot;
using FMOD.Studio;
using System.Collections.Generic;

[Tool]
public partial class FMODEditorProperty : EditorProperty, ISerializationListener
{
    private const string SCENE_PATH = "res://addons/DefluoLib/src/FMOD/FMODEditorProperty.tscn";

    private Control uiScene;
    private Button button;
    private Popup popup;
    private Tree tree;

    private string hintString;

    public FMODEditorProperty() { }

    public FMODEditorProperty(string hintString)
    {
        this.hintString = hintString;
    }

    public FMODResource CurrentResource;
    public string CurrentResourceName;

    public void OnBeforeSerialize()
    {
        // Disconnect all event handlers when reloading assemblies
        button.Pressed -= OnButtonPress;
        DefluoLib.FMODLister.Refreshed -= RefreshData;

        switch (hintString)
        {
            case "FMODEvent":
                tree.ItemSelected -= EventOnSelected;
                tree.ItemCollapsed -= EventOnCollapsed;
                break;

            case "FMODBus":
                tree.ItemSelected -= BusOnSelected;
                break;

            case "FMODVCA":
                tree.ItemSelected -= VCAOnSelected;
                break;

            case "FMODParameter":
                tree.ItemSelected -= ParameterOnSelected;
                break;
        }
        // Remove UI scene, it will be reinstantiated after reload
        RemoveChild(uiScene);
    }

    public void OnAfterDeserialize()
    {
        // Reinitialize editor property after reloading
        _Ready();
    }

    public override void _Ready()
    {
        uiScene = GD.Load<PackedScene>(SCENE_PATH).Instantiate<Control>();
        AddChild(uiScene);

        popup = uiScene.GetNode<Popup>("Popup");
        button = uiScene.GetNode<Button>("Button");
        tree = popup.GetNode<Tree>("Panel/Tree");

        popup.Visible = false;
        button.Pressed += OnButtonPress;

        // Run initialization based on resource type
        switch (hintString)
        {
            case "FMODEvent":
                button.Icon = DefluoLib.FMODIcons.EventIcon;
                tree.ItemSelected += EventOnSelected;
                tree.ItemCollapsed += EventOnCollapsed;
                break;

            case "FMODBus":
                button.Icon = DefluoLib.FMODIcons.ReturnBusIcon;
                tree.ItemSelected += BusOnSelected;
                break;

            case "FMODVCA":
                button.Icon = DefluoLib.FMODIcons.VCAIcon;
                tree.ItemSelected += VCAOnSelected;
                break;

            case "FMODParameter":
                button.Icon = DefluoLib.FMODIcons.ContinuousParameterIcon;
                tree.ItemSelected += ParameterOnSelected;
                break;
        }

        DefluoLib.FMODLister.Refreshed += RefreshData;
    }

    private void OnButtonPress()
    {
        popup.Visible = !popup.Visible;
    }

    public void RefreshData()
    {
        CurrentResource = (FMODResource)GetEditedObject().Get(GetEditedProperty());

        // If no resource has been created, the name is empty
        if (CurrentResource == null)
            CurrentResourceName = "";
        // The resource path bus:/ refers to the Master bus
        else if (CurrentResource.Path == "bus:/")
            CurrentResourceName = "Master";
        // Otherwise get the last part of the path which should be the resource name
        else
            CurrentResourceName = CurrentResource.Path.Split("/")[^1];

        // Clear possible TreeItems before initialization
        tree.Clear();

        switch (hintString)
        {
            case "FMODEvent":
                button.Text = popup.Title = "Select Event";
                InitEventTree(tree);
                break;

            case "FMODBus":
                button.Text = popup.Title = "Select Bus";
                button.Icon = DefluoLib.FMODIcons.ReturnBusIcon;
                if (CurrentResource is FMODBus bus)
                {
                    if (bus.Path == "bus:/")
                        button.Icon = DefluoLib.FMODIcons.MasterBusIcon;
                }
                InitBusTree(tree);
                break;

            case "FMODVCA":
                button.Text = popup.Title = "Select VCA";
                InitVCATree(tree);
                break;

            case "FMODParameter":
                button.Text = popup.Title = "Select Parameter";
                InitParameterTree(tree);
                break;
        }

        // If resource doesn't exist,
        // the button text should be the one initialized based on the resource type
        // Otherwise use the resource's name
        if (CurrentResourceName != "")
            button.Text = CurrentResourceName;
    }

    public override void _UpdateProperty()
    {
        RefreshData();
    }

    public static void InitEventTree(Tree tree)
    {
        var root = tree.CreateItem();
        var folders = new Dictionary<string, TreeItem>();
        foreach (var eventPath in DefluoLib.FMODLister.EventPaths)
        {
            var pathSteps = eventPath.Split('/');
            var folderItem = root;
            var folderPath = "";
            for (var i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var eventItem = tree.CreateItem(folderItem);
                    eventItem.SetText(0, pathSteps[i]);
                    eventItem.SetTooltipText(0, eventPath);
                    eventItem.SetIcon(0, DefluoLib.FMODIcons.EventIcon);
                }
                else
                {
                    folderPath += pathSteps[i] + "/";
                    if (folders.TryGetValue(folderPath, out var value))
                        folderItem = value;
                    else
                    {
                        folderItem = tree.CreateItem(folderItem);
                        folderItem.SetText(0, pathSteps[i]);
                        folderItem.SetIcon(0, DefluoLib.FMODIcons.FolderClosedIcon);
                        folderItem.Collapsed = true;
                        folderItem.SetSelectable(0, false);
                        folders.Add(folderPath, folderItem);
                    }
                }
            }
        }
    }

    private void EventOnSelected()
    {
        var item = tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        popup.Visible = false;
        button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODEvent(path));
    }

    private void EventOnCollapsed(TreeItem item)
    {
        if (item.Collapsed)
            item.SetIcon(0, DefluoLib.FMODIcons.FolderClosedIcon);
        else
            item.SetIcon(0, DefluoLib.FMODIcons.FolderOpenedIcon);
    }

    public static void InitBusTree(Tree tree)
    {
        var root = tree.CreateItem();
        var buses = new Dictionary<string, TreeItem>();

        var masterBus = tree.CreateItem(root);
        masterBus.SetText(0, "Master");
        masterBus.SetTooltipText(0, "bus:/");
        masterBus.SetIcon(0, DefluoLib.FMODIcons.MasterBusIcon);
        masterBus.Collapsed = false;
        buses.Add("bus:/", masterBus);

        foreach (var busPath in DefluoLib.FMODLister.BusPaths)
        {
            if (buses.ContainsKey(busPath))
                continue;

            var pathSteps = busPath.Split('/');
            var parent = masterBus;
            var parentPath = "bus:/";
            for (var i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var bus = tree.CreateItem(parent);
                    bus.SetText(0, pathSteps[i]);
                    bus.SetTooltipText(0, busPath);
                    bus.SetIcon(0, DefluoLib.FMODIcons.ReturnBusIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (buses.ContainsKey(parentPath))
                        continue;

                    parent = tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, DefluoLib.FMODIcons.GroupBusIcon);
                    parent.Collapsed = true;
                    buses.Add(busPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void BusOnSelected()
    {
        var item = tree.GetSelected();
        var path = item.GetTooltipText(0);

        if (path == "")
            return;

        if (path == "bus:/")
            button.Icon = DefluoLib.FMODIcons.MasterBusIcon;
        else
            button.Icon = DefluoLib.FMODIcons.ReturnBusIcon;

        popup.Visible = false;
        button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODBus(path));
    }

    public static void InitVCATree(Tree tree)
    {
        var root = tree.CreateItem();
        var vcas = new Dictionary<string, TreeItem>();

        foreach (var vcaPath in DefluoLib.FMODLister.VCAPaths)
        {
            if (vcas.ContainsKey(vcaPath))
                continue;

            var pathSteps = vcaPath.Split('/');
            var parent = root;
            var parentPath = "vca:/";
            for (var i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var vca = tree.CreateItem(parent);
                    vca.SetText(0, pathSteps[i]);
                    vca.SetTooltipText(0, vcaPath);
                    vca.SetIcon(0, DefluoLib.FMODIcons.VCAIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (vcas.ContainsKey(parentPath))
                        continue;

                    parent = tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, DefluoLib.FMODIcons.VCAIcon);
                    parent.Collapsed = true;
                    vcas.Add(vcaPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void VCAOnSelected()
    {
        var item = tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        popup.Visible = false;
        button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODVCA(path));
    }

    public static void InitParameterTree(Tree tree)
    {
        var root = tree.CreateItem();

        foreach (var parameterPath in DefluoLib.FMODLister.ParameterPaths)
        {
            var param = tree.CreateItem(root);
            param.SetText(0, parameterPath.Split("/")[^1]);
            param.SetTooltipText(0, parameterPath);

            DefluoLib.FMODLister.StudioSystem.getParameterDescriptionByName(
                parameterPath,
                out var paramDesc
            );

            GD.Print(paramDesc.flags);

            if (paramDesc.flags.HasFlag(PARAMETER_FLAGS.LABELED))
                param.SetIcon(0, DefluoLib.FMODIcons.LabeledParameterIcon);
            else if (paramDesc.flags.HasFlag(PARAMETER_FLAGS.DISCRETE))
                param.SetIcon(0, DefluoLib.FMODIcons.DiscreteParameterIcon);
            else
                param.SetIcon(0, DefluoLib.FMODIcons.ContinuousParameterIcon);
        }
    }

    private void ParameterOnSelected()
    {
        var item = tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        popup.Visible = false;
        button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODParameter(path));
    }
}
