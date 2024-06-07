using Godot;
using System;
using System.Linq;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_string.svg")]
internal partial class StringSettingTemplate : SettingTemplate
{
    [Export]
    public NodePath Selector;

    private SettingString Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingString)setting;
        var selector = GetNode<OptionButton>(Selector);

        selector.Clear();
        foreach (var possibleValue in Setting.PossibleValues)
        {
            selector.AddItem(possibleValue);
        }
        var selectedIndex = Array.FindIndex(
            Setting.PossibleValues,
            value => value == Setting.Value
        );
        selector.Select(selectedIndex);

        selector.ItemSelected += (index) =>
        {
            Setting.Value = Setting.PossibleValues[index];
        };

        Defluo.Settings.SettingsReset += () =>
        {
            selector.Select(
                Setting.PossibleValues.ToList().FindIndex(value => value == Setting.Value)
            );
        };
    }
}
