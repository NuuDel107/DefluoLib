using Godot;
using System;
using System.Linq;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_string.svg")]
internal partial class StringSettingTemplate : SettingTemplate
{
    [Export]
    public OptionButton Selector;

    private SettingString Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingString)setting;

        foreach (var possibleValue in Setting.PossibleValues)
        {
            Selector.AddItem(possibleValue);
        }
        var selectedIndex = Array.FindIndex(
            Setting.PossibleValues,
            value => value == Setting.Value
        );
        Selector.Select(selectedIndex);

        Selector.ItemSelected += (index) =>
        {
            Setting.Value = Setting.PossibleValues[index];
        };

        Defluo.Settings.SettingsReset += () =>
        {
            Selector.Select(
                Setting.PossibleValues.ToList().FindIndex(value => value == Setting.Value)
            );
        };
    }
}
