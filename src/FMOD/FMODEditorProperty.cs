using Godot;
using FMOD.Studio;
using System.Collections.Generic;

namespace DefluoLib;

public static class FMODIcons
{
    public static Texture2D EventIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/event_icon.png"
    );
    public static Texture2D FolderClosedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_closed.png"
    );
    public static Texture2D FolderOpenedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_opened.png"
    );
    public static Texture2D GroupBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/group_bus_icon.png"
    );
    public static Texture2D ReturnBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/return_bus_icon.png"
    );
    public static Texture2D MasterBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/master_bus_icon.png"
    );
    public static Texture2D VCAIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/vca_icon.png"
    );
    public static Texture2D ContinuousParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/c_parameter_icon.png"
    );
    public static Texture2D DiscreteParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/d_parameter_icon.png"
    );
    public static Texture2D LabeledParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/l_parameter_icon.png"
    );
    public static Texture2D BankIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/bank_icon.png"
    );
    public static Texture2D SnapshotIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/snapshot_icon.png"
    );
    public static Texture2D LogoIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/fmod_icon.svg"
    );
}

[Tool]
public partial class FMODEditorProperty : EditorProperty, ISerializationListener
{
    private const string scenePath = "res://addons/DefluoLib/src/FMOD/FMODEditorProperty.tscn";

    private Control UIScene;
    private Button Button;
    private Popup Popup;
    private Tree Tree;

    private string hintString;

    public FMODEditorProperty() { }

    public FMODEditorProperty(string hintString)
    {
        this.hintString = hintString;
    }

    public FMODResource currentResource;
    public string currentResourceName;

    public void OnBeforeSerialize()
    {
        // Disconnect all event handlers when reloading assemblies
        Button.Pressed -= OnButtonPress;
        DefluoLib.Singleton.FMODLister.Refreshed -= RefreshData;

        switch (hintString)
        {
            case "FMODEvent":
                Tree.ItemSelected -= EventOnSelected;
                Tree.ItemCollapsed -= EventOnCollapsed;
                break;

            case "FMODBus":
                Tree.ItemSelected -= BusOnSelected;
                break;

            case "FMODVCA":
                Tree.ItemSelected -= VCAOnSelected;
                break;

            case "FMODParameter":
                Tree.ItemSelected -= ParameterOnSelected;
                break;
        }
        // Remove UI scene, it will be reinstantiated after reload
        RemoveChild(UIScene);
    }

    public void OnAfterDeserialize()
    {
        // Reinitialize editor property after reloading
        _Ready();
    }

    public override void _Ready()
    {
        UIScene = GD.Load<PackedScene>(scenePath).Instantiate<Control>();
        AddChild(UIScene);

        Popup = UIScene.GetNode<Popup>("Popup");
        Button = UIScene.GetNode<Button>("Button");
        Tree = Popup.GetNode<Tree>("Panel/Tree");

        Popup.Visible = false;
        Button.Pressed += OnButtonPress;

        // Run initialization based on resource type
        switch (hintString)
        {
            case "FMODEvent":
                Button.Icon = FMODIcons.EventIcon;
                Tree.ItemSelected += EventOnSelected;
                Tree.ItemCollapsed += EventOnCollapsed;
                break;

            case "FMODBus":
                Button.Icon = FMODIcons.ReturnBusIcon;
                Tree.ItemSelected += BusOnSelected;
                break;

            case "FMODVCA":
                Button.Icon = FMODIcons.VCAIcon;
                Tree.ItemSelected += VCAOnSelected;
                break;

            case "FMODParameter":
                Button.Icon = FMODIcons.ContinuousParameterIcon;
                Tree.ItemSelected += ParameterOnSelected;
                break;
        }

        DefluoLib.Singleton.FMODLister.Refreshed += RefreshData;
    }

    private void OnButtonPress()
    {
        Popup.Visible = !Popup.Visible;
    }

    public void RefreshData()
    {
        currentResource = (FMODResource)GetEditedObject().Get(GetEditedProperty());

        // If no resource has been created, the name is empty
        if (currentResource == null)
            currentResourceName = "";
        // The resource path bus:/ refers to the Master bus
        else if (currentResource.Path == "bus:/")
            currentResourceName = "Master";
        // Otherwise get the last part of the path which should be the resource name
        else
            currentResourceName = currentResource.Path.Split("/")[^1];

        // Clear possible TreeItems before initialization
        Tree.Clear();

        switch (hintString)
        {
            case "FMODEvent":
                Button.Text = Popup.Title = "Select Event";
                InitEventTree(Tree);
                break;

            case "FMODBus":
                Button.Text = Popup.Title = "Select Bus";
                Button.Icon = FMODIcons.ReturnBusIcon;
                if (currentResource is FMODBus bus)
                {
                    if (bus.Path == "bus:/")
                        Button.Icon = FMODIcons.MasterBusIcon;
                }
                InitBusTree(Tree);
                break;

            case "FMODVCA":
                Button.Text = Popup.Title = "Select VCA";
                InitVCATree(Tree);
                break;

            case "FMODParameter":
                Button.Text = Popup.Title = "Select Parameter";
                InitParameterTree(Tree);
                break;
        }

        // If resource doesn't exist,
        // the button text should be the one initialized based on the resource type
        // Otherwise use the resource's name
        if (currentResourceName != "")
            Button.Text = currentResourceName;
    }

    public override void _UpdateProperty()
    {
        RefreshData();
    }

    public static void InitEventTree(Tree tree)
    {
        var root = tree.CreateItem();
        var folders = new Dictionary<string, TreeItem>();
        foreach (var eventPath in DefluoLib.Singleton.FMODLister.EventPaths)
        {
            var pathSteps = eventPath.Split('/');
            var folderItem = root;
            var folderPath = "";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var eventItem = tree.CreateItem(folderItem);
                    eventItem.SetText(0, pathSteps[i]);
                    eventItem.SetTooltipText(0, eventPath);
                    eventItem.SetIcon(0, FMODIcons.EventIcon);
                }
                else
                {
                    folderPath += pathSteps[i] + "/";
                    if (folders.ContainsKey(folderPath))
                        folderItem = folders[folderPath];
                    else
                    {
                        folderItem = tree.CreateItem(folderItem);
                        folderItem.SetText(0, pathSteps[i]);
                        folderItem.SetIcon(0, FMODIcons.FolderClosedIcon);
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
        var item = Tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        Popup.Visible = false;
        Button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODEvent(path));
    }

    private void EventOnCollapsed(TreeItem item)
    {
        if (item.Collapsed)
            item.SetIcon(0, FMODIcons.FolderClosedIcon);
        else
            item.SetIcon(0, FMODIcons.FolderOpenedIcon);
    }

    public static void InitBusTree(Tree tree)
    {
        var root = tree.CreateItem();
        var buses = new Dictionary<string, TreeItem>();

        var masterBus = tree.CreateItem(root);
        masterBus.SetText(0, "Master");
        masterBus.SetTooltipText(0, "bus:/");
        masterBus.SetIcon(0, FMODIcons.MasterBusIcon);
        masterBus.Collapsed = false;
        buses.Add("bus:/", masterBus);

        foreach (var busPath in DefluoLib.Singleton.FMODLister.BusPaths)
        {
            if (buses.ContainsKey(busPath))
                continue;

            var pathSteps = busPath.Split('/');
            var parent = masterBus;
            var parentPath = "bus:/";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var bus = tree.CreateItem(parent);
                    bus.SetText(0, pathSteps[i]);
                    bus.SetTooltipText(0, busPath);
                    bus.SetIcon(0, FMODIcons.ReturnBusIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (buses.ContainsKey(parentPath))
                        continue;

                    parent = tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, FMODIcons.GroupBusIcon);
                    parent.Collapsed = true;
                    buses.Add(busPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void BusOnSelected()
    {
        var item = Tree.GetSelected();
        var path = item.GetTooltipText(0);

        if (path == "")
            return;

        if (path == "bus:/")
            Button.Icon = FMODIcons.MasterBusIcon;
        else
            Button.Icon = FMODIcons.ReturnBusIcon;

        Popup.Visible = false;
        Button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODBus(path));
    }

    public static void InitVCATree(Tree tree)
    {
        var root = tree.CreateItem();
        var vcas = new Dictionary<string, TreeItem>();

        foreach (var VCAPath in DefluoLib.Singleton.FMODLister.VCAPaths)
        {
            if (vcas.ContainsKey(VCAPath))
                continue;

            var pathSteps = VCAPath.Split('/');
            var parent = root;
            var parentPath = "vca:/";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var VCA = tree.CreateItem(parent);
                    VCA.SetText(0, pathSteps[i]);
                    VCA.SetTooltipText(0, VCAPath);
                    VCA.SetIcon(0, FMODIcons.VCAIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (vcas.ContainsKey(parentPath))
                        continue;

                    parent = tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, FMODIcons.VCAIcon);
                    parent.Collapsed = true;
                    vcas.Add(VCAPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void VCAOnSelected()
    {
        var item = Tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        Popup.Visible = false;
        Button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODVCA(path));
    }

    public static void InitParameterTree(Tree tree)
    {
        var root = tree.CreateItem();

        foreach (var parameterPath in DefluoLib.Singleton.FMODLister.ParameterPaths)
        {
            var param = tree.CreateItem(root);
            param.SetText(0, parameterPath.Split("/")[^1]);
            param.SetTooltipText(0, parameterPath);

            DefluoLib.Singleton.FMODLister.StudioSystem.getParameterDescriptionByName(
                parameterPath,
                out var paramDesc
            );

            GD.Print(paramDesc.flags);

            if (paramDesc.flags.HasFlag(PARAMETER_FLAGS.LABELED))
                param.SetIcon(0, FMODIcons.LabeledParameterIcon);
            else if (paramDesc.flags.HasFlag(PARAMETER_FLAGS.DISCRETE))
                param.SetIcon(0, FMODIcons.DiscreteParameterIcon);
            else
                param.SetIcon(0, FMODIcons.ContinuousParameterIcon);
        }
    }

    private void ParameterOnSelected()
    {
        var item = Tree.GetSelected();
        var path = item.GetTooltipText(0);
        if (path == "")
            return;

        Popup.Visible = false;
        Button.Text = item.GetText(0);
        EmitChanged(GetEditedProperty(), new FMODParameter(path));
    }
}
