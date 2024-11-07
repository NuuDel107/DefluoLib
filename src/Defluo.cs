using Godot;
using System.Runtime.InteropServices;

namespace DefluoLib;

/// <summary>
/// Access point for the different classes in <c>DefluoLib</c>
/// </summary>
public partial class Defluo : Node
{
    /// <summary>
    /// Returns the scene tree of the currently running scene.
    /// </summary>
    /// <returns></returns>
    public static SceneTree GetSceneTree() => (SceneTree)Engine.GetMainLoop();

    /// <summary>
    /// Returns the autoload singleton of the main Defluo class
    /// </summary>
    public static Defluo Singleton
    {
        get { return GetSceneTree().Root.GetNode<Defluo>("Defluo"); }
    }

    /// <summary>
    /// Node that handles access to input actions and updates their states
    /// </summary>
    public static Input Input
    {
        get { return Singleton.GetNode<Input>("Input"); }
    }

    /// <summary>
    /// Node that keeps track of settings and can load and save them.
    /// </summary>
    public static Settings Settings
    {
        get { return Singleton.GetNode<Settings>("Settings"); }
    }

    /// <summary>
    /// Handler for interfacing with the FMOD Godot integration
    /// </summary>
    public static FMODHandler FMOD
    {
        get { return Singleton.GetNode<FMODHandler>("FMOD"); }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool SetDllDirectory(string lpPathName);

    public Defluo()
    {
        // Set the default directory for importing DLLs
        SetDllDirectory(ProjectSettings.GlobalizePath("res://addons/DefluoLib/bin"));
    }

    public override void _EnterTree()
    {
        AddChild(new Input());
        AddChild(new Settings());
        AddChild(new FMODHandler());
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Print("Initialization complete");
    }

    /// <summary>
    /// Prints a message to the console in DefluoLib formatting
    /// </summary>
    internal static void Print(string message)
    {
        GD.PrintRich(
            $"[b][lb]{StringFormat.Colored("Defluo", Colors.Aquamarine)}{StringFormat.Colored("Lib", Colors.Teal)}[rb][/b] {message}"
        );
    }

    /// <summary>
    /// Returns the current locale's translation for the given key
    /// </summary>
    /// <param name="key">String corresponding to the translation key</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string Tr(string key, string context = null) =>
        TranslationServer.Translate(key, context);

    /// <summary>
    /// Returns the current locale's translation for the given key in plural form if possible
    /// </summary>
    /// <param name="key">String corresponding to the translation key in singular form</param>
    /// <param name="pluralKey">String corresponding to the translation key in plural form</param>
    /// <param name="number">Quantity of the plural object, used to fetch the correct form</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string Tr(string key, string pluralKey, int number, string context = null) =>
        TranslationServer.TranslatePlural(key, pluralKey, number, context);
}
