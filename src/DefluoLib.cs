namespace DefluoLib;

using Godot;
using Godot.Collections;
using System.Runtime.InteropServices;

[Tool]
public partial class DefluoLib : EditorPlugin, ISerializationListener
{
    private const string MAIN_SCREEN_SCENE_PATH = "res://addons/DefluoLib/src/MainScreen.tscn";

    /// <summary>
    /// Returns the scene tree of the currently running scene.
    /// </summary>
    /// <returns></returns>
    public static SceneTree GetSceneTree() => (SceneTree)Engine.GetMainLoop();

    public static DefluoLib Singleton =>
        GetSceneTree().Root.GetChild(0).GetNode<DefluoLib>("DefluoLib");

    private FMODInspectorPlugin inspectorPlugin;
    private MainScreen mainScreen;

    private FMODLister fmodLister;
    public static FMODLister FMODLister => Singleton.fmodLister;

    private FMODIcons fmodIcons;
    public static FMODIcons FMODIcons => Singleton.fmodIcons;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    /// <summary>
    /// Sets the directory path in the project from which to load .dll libraries by default.
    /// </summary>
    /// <param name="directoryPath">The local directory path (starting with "res://")</param>
    /// <returns></returns>
    public static bool SetDllLoadDirectory(string directoryPath = "res://addons/DefluoLib/bin") =>
        SetDllDirectory(ProjectSettings.GlobalizePath(directoryPath));

    public DefluoLib()
    {
        // Set the default directory for importing DLLs
        SetDllLoadDirectory();
    }

    public void OnBeforeSerialize()
    {
        // Release the studio system when reloading assemblies
        // It will be reinitialized afterwards
        fmodLister.StudioSystem.release();
        // Remove main screen before reinitialization
        mainScreen?.QueueFree();
    }

    public void OnAfterDeserialize()
    {
        // Reinitialize plugin state after reloading
        _EnterTree();
    }

    public override void _EnterTree()
    {
        Name = "DefluoLib";
        AddAutoloadSingleton("Defluo", "res://addons/DefluoLib/src/Defluo.cs");

        fmodLister = new();
        fmodIcons = new();

        mainScreen = ResourceLoader
            .Load<PackedScene>(MAIN_SCREEN_SCENE_PATH)
            .Instantiate<MainScreen>();
        EditorInterface.Singleton.GetEditorMainScreen().AddChild(mainScreen);
        _MakeVisible(false);

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
        AddProjectSetting("DefluoLib/FMOD/EnableLiveUpdate", true, Variant.Type.Bool);
    }

    public override bool _HasMainScreen() => true;

    public override string _GetPluginName() => "DefluoLib";

    private Texture2D mainScreenLogo = ResourceLoader.Load<Texture2D>(
        "res://addons/DefluoLib/img/DefluoLib.svg"
    );

    public override Texture2D _GetPluginIcon() => mainScreenLogo;

    public override void _MakeVisible(bool visible)
    {
        if (mainScreen != null)
            mainScreen.Visible = visible;
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
        fmodLister.StudioSystem.release();
        RemoveInspectorPlugin(inspectorPlugin);
        mainScreen?.QueueFree();
    }
}
