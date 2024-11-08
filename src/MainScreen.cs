using Godot;
using Godot.Collections;
using FMOD.Studio;
using FMOD;

namespace DefluoLib;

[Tool]
public partial class MainScreen : Control
{
    [Export]
    public Array<Texture2D> MainTabIcons;

    [Export]
    public TabContainer TabContainer;

    [Export]
    public Button RefreshFMODButton;

    [Export]
    public Tree FMODTree;

    [Export]
    public Container EventPlaybackContainer;

    [Export]
    public Button EventPlayButton;

    [Export]
    public Texture2D PlayIcon;

    [Export]
    public Texture2D StopIcon;

    private string SelectedPath = "";
    private EventInstance? playingEventInstance;

    public override void _Ready()
    {
        foreach (var (icon, index) in MainTabIcons.WithIndex())
        {
            TabContainer?.SetTabIcon(index, icon);
        }

        InitTree();
        FMODTree.ItemSelected += TreeItemSelected;
        FMODTree.ItemCollapsed += TreeItemCollapsed;
        RefreshFMODButton.Pressed += Refresh;
        EventPlayButton.Pressed += ToggleEventPlayback;
        EventPlaybackContainer.Visible = false;
    }

    private void InitTree()
    {
        FMODTree.Clear();
        var root = FMODTree.CreateItem();

        foreach (var bank in DefluoLib.Singleton.FMODLister.Banks)
        {
            var bankItem = FMODTree.CreateItem(root);
            FMODCaller.CheckResult(bank.getPath(out var bankPath));
            bankItem.SetText(0, bankPath.Split("/")[^1]);
            bankItem.SetTooltipText(0, bankPath);
            bankItem.SetIcon(0, FMODIcons.BankIcon);
            bankItem.Collapsed = true;

            ListEvents(bank, bankItem);
            ListBuses(bank, bankItem);
            ListVCAs(bank, bankItem);
        }
    }

    private void ListEvents(Bank bank, TreeItem parent)
    {
        FMODCaller.CheckResult(bank.getEventList(out var eventList));
        if (eventList.Length == 0)
            return;

        var eventParent = FMODTree.CreateItem(parent);
        eventParent.SetText(0, "Events");
        eventParent.Collapsed = true;
        eventParent.SetSelectable(0, false);

        TreeItem snapshotParent = null;

        var eventFolders = new Dictionary<string, TreeItem>();
        var snapshotFolders = new Dictionary<string, TreeItem>();
        foreach (var eventDesc in eventList)
        {
            FMODCaller.CheckResult(eventDesc.getPath(out var eventPath));
            FMODCaller.CheckResult(eventDesc.isSnapshot(out var isSnapshot));

            // Create snapshot treeitem only if bank has snapshot events
            if (snapshotParent == null && isSnapshot)
            {
                snapshotParent = FMODTree.CreateItem(parent);
                snapshotParent.SetText(0, "Snapshots");
                snapshotParent.Collapsed = true;
                snapshotParent.SetSelectable(0, false);
            }

            var pathSteps = eventPath.Split('/');
            var folderItem = isSnapshot ? snapshotParent : eventParent;
            var folderPath = "";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var eventItem = FMODTree.CreateItem(folderItem);
                    eventItem.SetText(0, pathSteps[i]);
                    eventItem.SetTooltipText(0, eventPath);
                    eventItem.SetIcon(0, isSnapshot ? FMODIcons.SnapshotIcon : FMODIcons.EventIcon);
                    eventItem.Collapsed = true;

                    eventDesc.getParameterDescriptionCount(out var paramAmount);
                    for (int paramId = 0; paramId < paramAmount; paramId++)
                    {
                        FMODCaller.CheckResult(
                            eventDesc.getParameterDescriptionByIndex(paramId, out var param)
                        );

                        var paramItem = FMODTree.CreateItem(eventItem);
                        paramItem.SetText(0, param.name);
                        if (param.flags.HasFlag(PARAMETER_FLAGS.LABELED))
                            paramItem.SetIcon(0, FMODIcons.LabeledParameterIcon);
                        else if (param.flags.HasFlag(PARAMETER_FLAGS.DISCRETE))
                            paramItem.SetIcon(0, FMODIcons.DiscreteParameterIcon);
                        else
                            paramItem.SetIcon(0, FMODIcons.ContinuousParameterIcon);
                    }
                }
                else
                {
                    folderPath += pathSteps[i] + "/";
                    if (isSnapshot)
                    {
                        if (snapshotFolders.ContainsKey(folderPath))
                            folderItem = snapshotFolders[folderPath];
                        else
                        {
                            folderItem = FMODTree.CreateItem(folderItem, snapshotFolders.Count);
                            folderItem.SetText(0, pathSteps[i]);
                            folderItem.SetTooltipText(0, folderPath);
                            folderItem.SetIcon(0, FMODIcons.FolderClosedIcon);
                            folderItem.Collapsed = true;
                            folderItem.SetSelectable(0, false);
                            snapshotFolders.Add(folderPath, folderItem);
                        }
                    }
                    else
                    {
                        if (eventFolders.ContainsKey(folderPath))
                            folderItem = eventFolders[folderPath];
                        else
                        {
                            folderItem = FMODTree.CreateItem(folderItem, eventFolders.Count);
                            folderItem.SetText(0, pathSteps[i]);
                            folderItem.SetTooltipText(0, folderPath);
                            folderItem.SetIcon(0, FMODIcons.FolderClosedIcon);
                            folderItem.Collapsed = true;
                            folderItem.SetSelectable(0, false);
                            eventFolders.Add(folderPath, folderItem);
                        }
                    }
                }
            }
        }
    }

    private void ListBuses(Bank bank, TreeItem parent)
    {
        FMODCaller.CheckResult(bank.getBusList(out var busList));
        if (busList.Length == 0)
            return;

        var busParent = FMODTree.CreateItem(parent);
        busParent.SetText(0, "Buses");
        busParent.Collapsed = true;
        busParent.SetSelectable(0, false);

        var busItems = new Dictionary<string, TreeItem>();

        var masterBus = FMODTree.CreateItem(busParent);
        masterBus.SetText(0, "Master");
        masterBus.SetTooltipText(0, "bus:/");
        masterBus.SetIcon(0, FMODIcons.MasterBusIcon);
        masterBus.Collapsed = true;
        busItems.Add("bus:/", masterBus);

        foreach (var bus in busList)
        {
            FMODCaller.CheckResult(bus.getPath(out var busPath));
            if (busItems.ContainsKey(busPath))
                continue;

            var pathSteps = busPath.Split('/');
            var parentBus = masterBus;
            var parentPath = "bus:/";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var busItem = FMODTree.CreateItem(parentBus);
                    busItem.SetText(0, pathSteps[i]);
                    busItem.SetTooltipText(0, busPath);
                    busItem.SetIcon(0, FMODIcons.ReturnBusIcon);
                    busItem.Collapsed = true;
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (busItems.ContainsKey(parentPath))
                        continue;

                    parentBus = FMODTree.CreateItem(parentBus);
                    parentBus.SetText(0, pathSteps[i]);
                    parentBus.SetTooltipText(0, parentPath);
                    parentBus.SetIcon(0, FMODIcons.GroupBusIcon);
                    parentBus.Collapsed = true;
                    busItems.Add(busPath, parentBus);

                    parentPath += "/";
                }
            }
        }
    }

    private void ListVCAs(Bank bank, TreeItem parent)
    {
        FMODCaller.CheckResult(bank.getVCAList(out var VCAlist));
        if (VCAlist.Length == 0)
            return;

        var vcaParent = FMODTree.CreateItem(parent);
        vcaParent.SetText(0, "VCAs");
        vcaParent.Collapsed = true;
        vcaParent.SetSelectable(0, false);

        var vcas = new Dictionary<string, TreeItem>();

        foreach (var VCA in VCAlist)
        {
            FMODCaller.CheckResult(VCA.getPath(out var VCAPath));
            if (vcas.ContainsKey(VCAPath))
                continue;

            var pathSteps = VCAPath.Split('/');
            var parentVCA = vcaParent;
            var parentPath = "vca:/";
            for (int i = 1; i < pathSteps.Length; i++)
            {
                if (i == pathSteps.Length - 1)
                {
                    var VCAItem = FMODTree.CreateItem(parentVCA);
                    VCAItem.SetText(0, pathSteps[i]);
                    VCAItem.SetTooltipText(0, VCAPath);
                    VCAItem.SetIcon(0, FMODIcons.VCAIcon);
                    VCAItem.Collapsed = true;
                }
                else
                {
                    parentPath += pathSteps[i];

                    if (vcas.ContainsKey(parentPath))
                        continue;

                    parentVCA = FMODTree.CreateItem(parentVCA);
                    parentVCA.SetText(0, pathSteps[i]);
                    parentVCA.SetTooltipText(0, parentPath);
                    parentVCA.SetIcon(0, FMODIcons.VCAIcon);
                    parentVCA.Collapsed = true;
                    vcas.Add(VCAPath, parentVCA);

                    parentPath += "/";
                }
            }
        }
    }

    private void TreeItemSelected()
    {
        var item = FMODTree.GetSelected();
        SelectedPath = item.GetTooltipText(0);
        var itemType = SelectedPath.Split("/")[0];

        ClearPlayingEventInstance();
        EventPlaybackContainer.Visible = itemType == "event:";
    }

    private void TreeItemCollapsed(TreeItem item)
    {
        ClearPlayingEventInstance();
        FMODTree.DeselectAll();
        // If last character of tooltip text is '/',
        // item is a folder
        var tooltip = item.GetTooltipText(0);
        if (tooltip.EndsWith('/') && tooltip != "bus:/")
        {
            if (item.Collapsed)
                item.SetIcon(0, FMODIcons.FolderClosedIcon);
            else
                item.SetIcon(0, FMODIcons.FolderOpenedIcon);
        }
    }

    private void ToggleEventPlayback()
    {
        if (playingEventInstance == null)
        {
            FMODCaller.CheckResult(
                DefluoLib.Singleton.FMODLister.StudioSystem.getEvent(
                    SelectedPath,
                    out var eventDesc
                )
            );
            eventDesc.createInstance(out var instance);
            instance.start();
            instance.setCallback(EventStoppedCallback, EVENT_CALLBACK_TYPE.SOUND_STOPPED);
            playingEventInstance = instance;
            DefluoLib.Singleton.FMODLister.StudioSystem.update();

            EventPlayButton.Icon = StopIcon;
        }
        else
        {
            playingEventInstance.Value.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.STOPPING)
            {
                ClearPlayingEventInstance();
            }
            else
            {
                playingEventInstance.Value.stop(STOP_MODE.ALLOWFADEOUT);
                DefluoLib.Singleton.FMODLister.StudioSystem.update();
            }
        }
    }

    private void ClearPlayingEventInstance()
    {
        if (playingEventInstance == null)
            return;

        playingEventInstance.Value.stop(STOP_MODE.IMMEDIATE);
        playingEventInstance.Value.release();
        playingEventInstance = null;
        DefluoLib.Singleton.FMODLister.StudioSystem.update();

        EventPlayButton.Icon = PlayIcon;
    }

    private RESULT EventStoppedCallback(
        EVENT_CALLBACK_TYPE type,
        nint eventPointer,
        nint parameterPointer
    )
    {
        CallDeferred("ClearPlayingEventInstance");
        return RESULT.OK;
    }

    private void Refresh()
    {
        DefluoLib.Singleton.FMODLister.Refresh();
        InitTree();
    }
}
