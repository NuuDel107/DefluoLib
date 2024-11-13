namespace DefluoLib;

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class SettingUI : HBoxContainer
{
    [Export]
    public Array<Control> UIElements = [];

    public int TargetSettingIndex;

    public VariantSetting Setting;

    private List<PropertyInfo> settingProperties;
    private string[] settingNames;

    public override Array<Dictionary> _GetPropertyList()
    {
        RefreshSettingsList();

        var propertyList = new Array<Dictionary>
        {
            new()
            {
                { "name", "TargetSettingIndex" },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", " ," + settingNames.Join(",") }
            }
        };
        return propertyList;
    }

    private void UpdateValue(Variant value)
    {
        if (value.VariantType != Setting.Type)
        {
            GD.PushError("Invalid value type for setting UI");
            return;
        }
        Setting.Value = value;
        ValueChanged?.Invoke(value);
    }

    private event Action<Variant> ValueChanged;

    private void RefreshSettingsList()
    {
        settingProperties = Settings.GetSettingProperties();
        settingNames = settingProperties.Select(property => property.Name).ToArray();
    }

    public override void _Ready()
    {
        RefreshSettingsList();

        if (Engine.IsEditorHint() || TargetSettingIndex == 0)
            return;

        Setting = Defluo.Settings.GetSettings()[settingNames[TargetSettingIndex - 1]];

        foreach (var element in UIElements)
        {
            switch (element)
            {
                case Godot.Range range:
                    range.ValueChanged += (value) => UpdateValue((float)value);
                    ValueChanged += (value) => range.SetValueNoSignal(value.As<float>());
                    break;
                case Label label:
                    ValueChanged += (value) => label.Text = value.As<string>();
                    break;
                case OptionButton dropdown:
                    dropdown.ItemSelected += (item) =>
                    {
                        UpdateValue((int)item);
                    };
                    ValueChanged += (value) => dropdown.Select(value.As<int>());
                    break;
                case LineEdit textBox:
                    textBox.TextSubmitted += (text) => UpdateValue(text);
                    ValueChanged += (text) => textBox.Text = text.As<string>();
                    break;
                case CheckBox checkBox:
                    checkBox.Toggled += value => UpdateValue(checkBox.ButtonPressed);
                    break;
                case CheckButton checkButton:
                    checkButton.Toggled += value => UpdateValue(checkButton.ButtonPressed);
                    break;
            }
        }
        UpdateValue(Setting.Value);
    }
}
