using Godot;

namespace DefluoLib;

[GlobalClass]
internal partial class SettingsPopulator : Control
{
    [ExportGroup("Base Nodes")]
    [Export]
    public Container SettingsContainer;

    [Export]
    public BoolSettingTemplate BoolSettingTemplate;

    [Export]
    public StringSettingTemplate StringSettingTemplate;

    [Export]
    public FloatSettingTemplate FloatSettingTemplate;

    [ExportGroup("Categories")]
    [Export]
    public TabContainer CategoryContainer;

    public bool CategoriesEnabled
    {
        get { return CategoryContainer != null; }
    }

    public override void _Ready()
    {
        BoolSettingTemplate.GetParent().RemoveChild(BoolSettingTemplate);
        FloatSettingTemplate.GetParent().RemoveChild(FloatSettingTemplate);
        StringSettingTemplate.GetParent().RemoveChild(StringSettingTemplate);

        var latestCategory = "";
        foreach (var setting in Defluo.Settings.GetSettings())
        {
            var template = GetSettingTemplate(setting);
            template.Name = setting.DisplayName;
            if (CategoriesEnabled)
            {
                if (setting.Category != latestCategory)
                {
                    latestCategory = setting.Category;
                    var container = SettingsContainer.Duplicate();
                    container.Name = latestCategory;
                    CategoryContainer.AddChild(container);
                }
                CategoryContainer.GetNode(latestCategory).AddChild(template);
            }
            else
                SettingsContainer.AddChild(template);
            template.Initialize(setting);
        }
        if (CategoriesEnabled)
            SettingsContainer.QueueFree();
    }

    private SettingTemplate GetSettingTemplate(SettingBase setting)
    {
        SettingTemplate template = null;
        switch (setting)
        {
            case SettingBool:
                template = (BoolSettingTemplate)BoolSettingTemplate.Duplicate();
                break;
            case SettingString:
                template = (StringSettingTemplate)StringSettingTemplate.Duplicate();
                break;
            case SettingFloat:
                template = (FloatSettingTemplate)FloatSettingTemplate.Duplicate();
                break;
        }
        template.Name = setting.DisplayName;
        return template;
    }
}
