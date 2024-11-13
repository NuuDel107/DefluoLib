namespace DefluoLib;

using Godot;

public class FMODIcons
{
    public Texture2D EventIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/event_icon.png"
    );
    public Texture2D FolderClosedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_closed.png"
    );
    public Texture2D FolderOpenedIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/folder_opened.png"
    );
    public Texture2D GroupBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/group_bus_icon.png"
    );
    public Texture2D ReturnBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/return_bus_icon.png"
    );
    public Texture2D MasterBusIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/master_bus_icon.png"
    );
    public Texture2D VCAIcon = GD.Load<Texture2D>("res://addons/DefluoLib/img/FMOD/vca_icon.png");
    public Texture2D ContinuousParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/c_parameter_icon.png"
    );
    public Texture2D DiscreteParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/d_parameter_icon.png"
    );
    public Texture2D LabeledParameterIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/l_parameter_icon.png"
    );
    public Texture2D BankIcon = GD.Load<Texture2D>("res://addons/DefluoLib/img/FMOD/bank_icon.png");
    public Texture2D SnapshotIcon = GD.Load<Texture2D>(
        "res://addons/DefluoLib/img/FMOD/snapshot_icon.png"
    );
    public Texture2D LogoIcon = GD.Load<Texture2D>("res://addons/DefluoLib/img/FMOD/fmod_icon.svg");
}
