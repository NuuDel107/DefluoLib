using Godot;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_float.svg")]
internal partial class FloatSettingTemplate : SettingTemplate
{
    [Export]
    public NodePath SliderSelector;

    [Export]
    public NodePath SpinBoxSelector;

    private SettingFloat Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingFloat)setting;
        var sliderSelector = GetNode<Slider>(SliderSelector);
        var spinBoxSelector = GetNode<SpinBox>(SpinBoxSelector);

        if (sliderSelector != null)
        {
            sliderSelector.MinValue = Setting.Constraints.Min;
            sliderSelector.MaxValue = Setting.Constraints.Max;
            sliderSelector.Step = Setting.Constraints.Step;
            sliderSelector.Value = Setting.Value;
        }
        if (spinBoxSelector != null)
        {
            if (Setting.IsPercentage)
            {
                spinBoxSelector.Suffix = "%";
                spinBoxSelector.MinValue = Setting.Constraints.Min * 100;
                spinBoxSelector.MaxValue = Setting.Constraints.Max * 100;
                spinBoxSelector.Step = Setting.Constraints.Step * 100;
                spinBoxSelector.Value = Setting.Value * 100;
            }
            else
            {
                spinBoxSelector.MinValue = Setting.Constraints.Min;
                spinBoxSelector.MaxValue = Setting.Constraints.Max;
                spinBoxSelector.Step = Setting.Constraints.Step;
                spinBoxSelector.Value = Setting.Value;
            }
        }

        sliderSelector.ValueChanged += (value) =>
        {
            Setting.Value = (float)value;
            spinBoxSelector?.SetValueNoSignal(value * (Setting.IsPercentage ? 100 : 1));
        };

        spinBoxSelector.ValueChanged += (value) =>
        {
            if (Setting.IsPercentage)
            {
                Setting.Value = (float)value / 100;
                sliderSelector?.SetValueNoSignal(value / 100);
            }
            else
            {
                Setting.Value = (float)value;
                sliderSelector?.SetValueNoSignal(value);
            }
        };

        Defluo.Settings.SettingsReset += () =>
        {
            sliderSelector.Value = Setting.Value;
        };
    }
}
