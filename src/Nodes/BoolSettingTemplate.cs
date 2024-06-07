using Godot;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_bool.svg")]
internal partial class BoolSettingTemplate : SettingTemplate
{
    [Export]
    public NodePath Selector;

    private SettingBool Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingBool)setting;
        var selector = GetNode<Button>(Selector);
        selector.ButtonPressed = Setting.Value;

        selector.Toggled += (value) =>
        {
            Setting.Value = value;
        };

        Defluo.Settings.SettingsReset += () =>
        {
            selector.SetPressedNoSignal(Setting.Value);
        };
    }
}
