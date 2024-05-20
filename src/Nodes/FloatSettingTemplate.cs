using Godot;

namespace DefluoLib;

[GlobalClass, Icon("res://addons/DefluoLib/img/icon_float.svg")]
internal partial class FloatSettingTemplate : SettingTemplate
{
    [Export]
    public Slider SliderSelector;

    [Export]
    public SpinBox SpinBoxSelector;

    private SettingFloat Setting;

    public override void Initialize(SettingBase setting)
    {
        base.Initialize(setting);
        Setting = (SettingFloat)setting;

        if (SliderSelector != null)
        {
            SliderSelector.MinValue = Setting.Constraints.Min;
            SliderSelector.MaxValue = Setting.Constraints.Max;
            SliderSelector.Step = Setting.Constraints.Step;
            SliderSelector.Value = Setting.Value;
        }
        if (SpinBoxSelector != null)
        {
            if (Setting.IsPercentage)
            {
                SpinBoxSelector.Suffix = "%";
                SpinBoxSelector.MinValue = Setting.Constraints.Min * 100;
                SpinBoxSelector.MaxValue = Setting.Constraints.Max * 100;
                SpinBoxSelector.Step = Setting.Constraints.Step * 100;
                SpinBoxSelector.Value = Setting.Value * 100;
            }
            else
            {
                SpinBoxSelector.MinValue = Setting.Constraints.Min;
                SpinBoxSelector.MaxValue = Setting.Constraints.Max;
                SpinBoxSelector.Step = Setting.Constraints.Step;
                SpinBoxSelector.Value = Setting.Value;
            }
        }

        SliderSelector.ValueChanged += (value) =>
        {
            Setting.Value = (float)value;
            SpinBoxSelector?.SetValueNoSignal(value * (Setting.IsPercentage ? 100 : 1));
        };

        SpinBoxSelector.ValueChanged += (value) =>
        {
            if (Setting.IsPercentage)
            {
                Setting.Value = (float)value / 100;
                SliderSelector?.SetValueNoSignal(value / 100);
            }
            else
            {
                Setting.Value = (float)value;
                SliderSelector?.SetValueNoSignal(value);
            }
        };

        Defluo.Settings.SettingsReset += () =>
        {
            SliderSelector.Value = Setting.Value;
        };
    }
}
