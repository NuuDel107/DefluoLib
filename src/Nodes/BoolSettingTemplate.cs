using Godot;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_bool.svg")]
internal partial class BoolSettingTemplate : SettingTemplate
{
    [Export]
    public Button Selector;

    private SettingBool Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingBool)setting;
        Selector.ButtonPressed = Setting.Value;

        Selector.Toggled += (value) =>
        {
            Setting.Value = value;
        };

        Defluo.Settings.SettingsReset += () =>
        {
            Selector.SetPressedNoSignal(Setting.Value);
        };
    }
}
