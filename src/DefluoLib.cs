#if TOOLS
using Godot;
using Godot.Collections;

namespace DefluoLib;

[Tool]
internal partial class DefluoLib : EditorPlugin
{
    public override void _EnterTree()
    {
        AddAutoloadSingleton("Defluo", "res://addons/DefluoLib/src/Defluo.cs");
        AddProjectSetting(
            "DefluoLib/Input/KeyboardIconFolder",
            "res://addons/DefluoLib/img/kenney_input/Keyboard",
            Variant.Type.String,
            PropertyHint.Dir
        );
        AddProjectSetting(
            "DefluoLib/Input/MouseIconFolder",
            "res://addons/DefluoLib/img/kenney_input/Mouse",
            Variant.Type.String,
            PropertyHint.Dir
        );
        AddProjectSetting(
            "DefluoLib/Input/XboxIconFolder",
            "res://addons/DefluoLib/img/kenney_input/Xbox",
            Variant.Type.String,
            PropertyHint.Dir
        );
        AddProjectSetting(
            "DefluoLib/Input/PlayStationIconFolder",
            "res://addons/DefluoLib/img/kenney_input/PlayStation",
            Variant.Type.String,
            PropertyHint.Dir
        );
        AddProjectSetting("DefluoLib/Input/InputLogging", false, Variant.Type.Bool);
        AddProjectSetting("DefluoLib/Input/EventLogging", false, Variant.Type.Bool);
    }

    public static void AddProjectSetting(
        string propertyName,
        Variant initialValue,
        Variant.Type propertyType = Variant.Type.Nil,
        PropertyHint propertyHint = PropertyHint.None,
        string propertyHintString = ""
    )
    {
        if (!ProjectSettings.HasSetting(propertyName))
            ProjectSettings.SetSetting(propertyName, initialValue);
        ProjectSettings.AddPropertyInfo(
            new Dictionary
            {
                { "name", propertyName },
                { "type", (int)propertyType },
                { "hint", (int)propertyHint },
                { "hint_string", propertyHintString }
            }
        );
        ProjectSettings.Save();
    }

    public override void _ExitTree() { }
}
#endif
