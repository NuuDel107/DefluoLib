using System.Linq;
using Godot;
using Godot.Collections;

namespace DefluoLib;

[Tool]
[GlobalClass]
internal abstract partial class SettingTemplate : Control
{
    [Export]
    public Label NameLabel;

    public int SettingIndex = 0;

    public override Array<Dictionary> _GetPropertyList()
    {
        var settingProperties = Settings
            .GetSettingProperties()
            .Select(property => property.Name)
            .ToArray();

        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "SettingIndex" },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", " ," + settingProperties.Join(",") }
            }
        };
        return properties;
    }

    public virtual void Initialize(SettingBase setting)
    {
        NameLabel.Text = setting.DisplayName;
    }
}
