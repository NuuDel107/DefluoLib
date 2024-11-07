#if TOOLS
using Godot;
using Godot.Collections;

namespace DefluoLib;

[Tool]
internal partial class DefluoLib : EditorPlugin
{
    /// <summary>
    /// Returns the scene tree of the currently running scene.
    /// </summary>
    /// <returns></returns>
    public static SceneTree GetSceneTree() => (SceneTree)Engine.GetMainLoop();

    public static DefluoLib Singleton =>
        GetSceneTree().Root.GetChild(0).GetNode<DefluoLib>("DefluoLib");

    private FMODInspectorPlugin inspectorPlugin;

    public FMODLister FMODLister;

    public DefluoLib() { }

    public override void _EnterTree()
    {
        Name = "DefluoLib";
        AddAutoloadSingleton("Defluo", "res://addons/DefluoLib/src/Defluo.cs");

        inspectorPlugin = new();
        AddInspectorPlugin(inspectorPlugin);

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

        AddProjectSetting(
            "DefluoLib/FMOD/BankFolder",
            "res://",
            Variant.Type.String,
            PropertyHint.Dir
        );

        AddProjectSetting(
            "DefluoLib/FMOD/BanksToLoadOnStartup",
            new Array(),
            Variant.Type.Array,
            PropertyHint.TypeString,
            $"{Variant.Type.String:D}/{PropertyHint.File:D}:*.bank"
        );

        FMODLister = new();
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

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(inspectorPlugin);
        FMODLister.StudioSystem.release();
    }
}
#endif
