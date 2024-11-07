using Godot;
using System.Collections.Generic;

namespace DefluoLib;

[Tool]
public partial class FMODEditorProperty : EditorProperty
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

    private Texture2D eventIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/event_icon.svg"
    );
    private Texture2D folderClosedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_closed.svg"
    );
    private Texture2D folderOpenedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_opened.svg"
    );
    private Texture2D busIcon = GD.Load<Texture2D>("res://addons/DefluoLib/img/FMOD/bus_icon.svg");
    private Texture2D VCAIcon = GD.Load<Texture2D>("res://addons/DefluoLib/img/FMOD/vca_icon.svg");
    private Texture2D parameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/c_parameter_icon.svg"
    );

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UIScene = GD.Load<PackedScene>(scenePath).Instantiate<Control>();
        AddChild(UIScene);

        Popup = UIScene.GetNode<Popup>("Popup");
        Button = UIScene.GetNode<Button>("Button");
        Tree = Popup.GetNode<Tree>("Panel/Tree");

        Popup.Visible = false;
        Button.Pressed += () =>
        {
            Popup.Visible = !Popup.Visible;
        };

        switch (hintString)
        {
            case "FMODEvent":
                Button.Icon = eventIcon;
                Tree.ItemSelected += () =>
                {
                    var item = Tree.GetSelected();
                    var path = item.GetTooltipText(0);
                    if (path == "")
                        return;

                    Popup.Visible = false;
                    Button.Text = item.GetText(0);
                    EmitChanged(GetEditedProperty(), new FMODEvent(path));
                };

                Tree.ItemCollapsed += item =>
                {
                    if (item.Collapsed)
                        item.SetIcon(0, folderClosedIcon);
                    else
                        item.SetIcon(0, folderOpenedIcon);
                };
                break;

            case "FMODBus":
                Button.Icon = busIcon;
                Tree.ItemSelected += () =>
                {
                    var item = Tree.GetSelected();
                    var path = item.GetTooltipText(0);

                    if (path == "")
                        return;

                    Popup.Visible = false;
                    Button.Text = item.GetText(0);
                    EmitChanged(GetEditedProperty(), new FMODBus(path));
                };
                break;

            case "FMODVCA":
                Button.Icon = VCAIcon;
                Tree.ItemSelected += () =>
                {
                    var item = Tree.GetSelected();
                    var path = item.GetTooltipText(0);
                    if (path == "")
                        return;

                    Popup.Visible = false;
                    Button.Text = item.GetText(0);
                    EmitChanged(GetEditedProperty(), new FMODVCA(path));
                };
                break;

            case "FMODParameter":
                Button.Icon = parameterIcon;
                Tree.ItemSelected += () =>
                {
                    var item = Tree.GetSelected();
                    var path = item.GetTooltipText(0);
                    if (path == "")
                        return;

                    Popup.Visible = false;
                    Button.Text = item.GetText(0);
                    EmitChanged(GetEditedProperty(), new FMODParameter(path));
                };
                break;
        }
    }

    public void RefreshData()
    {
        currentResource = (FMODResource)GetEditedObject().Get(GetEditedProperty());
        if (currentResource == null)
            currentResourceName = "";
        else if (currentResource.Path == "bus:/")
            currentResourceName = "Master";
        else
            currentResourceName = currentResource.Path.Split("/")[^1];

        Tree.Clear();

        switch (hintString)
        {
            case "FMODEvent":
                Button.Text = "Select Event";
                InitEventTree();
                break;

            case "FMODBus":
                Button.Text = "Select Bus";
                InitBusTree();
                break;

            case "FMODVCA":
                Button.Text = "Select VCA";
                InitVCATree();
                break;

            case "FMODParameter":
                Button.Text = "Select Parameter";
                InitParameterTree();
                break;
        }

        if (currentResourceName != "")
            Button.Text = currentResourceName;
    }

    public override void _UpdateProperty()
    {
        RefreshData();
    }

    private void InitEventTree()
    {
        Popup.Title = "Select Event";

        var root = Tree.CreateItem();
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
                    var eventItem = Tree.CreateItem(folderItem);
                    eventItem.SetText(0, pathSteps[i]);
                    eventItem.SetTooltipText(0, eventPath);
                    eventItem.SetIcon(0, eventIcon);
                }
                else
                {
                    folderPath += pathSteps[i] + "/";
                    if (folders.ContainsKey(folderPath))
                        folderItem = folders[folderPath];
                    else
                    {
                        folderItem = Tree.CreateItem(folderItem);
                        folderItem.SetText(0, pathSteps[i]);
                        folderItem.SetIcon(0, folderClosedIcon);
                        folderItem.Collapsed = true;
                        folderItem.SetSelectable(0, false);
                        folders.Add(folderPath, folderItem);
                    }
                }
            }
        }
    }

    private void InitBusTree()
    {
        Popup.Title = "Select Bus";

        var root = Tree.CreateItem();
        var buses = new Dictionary<string, TreeItem>();

        var masterBus = Tree.CreateItem(root);
        masterBus.SetText(0, "Master");
        masterBus.SetTooltipText(0, "bus:/");
        masterBus.SetIcon(0, busIcon);
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
                    var bus = Tree.CreateItem(parent);
                    bus.SetText(0, pathSteps[i]);
                    bus.SetTooltipText(0, busPath);
                    bus.SetIcon(0, busIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (buses.ContainsKey(parentPath))
                        continue;

                    parent = Tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, busIcon);
                    parent.Collapsed = true;
                    buses.Add(busPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void InitVCATree()
    {
        Popup.Title = "Select VCA";

        var root = Tree.CreateItem();
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
                    var VCA = Tree.CreateItem(parent);
                    VCA.SetText(0, pathSteps[i]);
                    VCA.SetTooltipText(0, VCAPath);
                    VCA.SetIcon(0, VCAIcon);
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (vcas.ContainsKey(parentPath))
                        continue;

                    parent = Tree.CreateItem(parent);
                    parent.SetText(0, pathSteps[i]);
                    parent.SetTooltipText(0, parentPath);
                    parent.SetIcon(0, VCAIcon);
                    parent.Collapsed = true;
                    vcas.Add(VCAPath, parent);

                    parentPath += "/";
                }
            }
        }
    }

    private void InitParameterTree()
    {
        Popup.Title = "Select Parameter";

        var root = Tree.CreateItem();

        foreach (var parameterPath in DefluoLib.Singleton.FMODLister.ParameterPaths)
        {
            var param = Tree.CreateItem(root);
            param.SetText(0, parameterPath.Split("/")[^1]);
            param.SetTooltipText(0, parameterPath);
            param.SetIcon(0, parameterIcon);
        }
    }
}
