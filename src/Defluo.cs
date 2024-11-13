namespace DefluoLib;

using Godot;

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
    /// Returns the autoload singleton of the main Defluo class. <br/>Can be <c>null</c> if autoload hasn't initialized yet.
    /// </summary>
    public static Defluo Singleton => GetSceneTree().Root.GetNodeOrNull<Defluo>("Defluo");

    /// <summary>
    /// Node that handles access to input actions and updates their states. <br/>Can be <c>null</c> if autoload hasn't initialized yet.
    /// </summary>
    public static Input Input => Singleton.GetNodeOrNull<Input>("Input");

    /// <summary>
    /// Node that keeps track of settings and can load and save them. <br/>Can be <c>null</c> if autoload hasn't initialized yet.
    /// </summary>
    public static Settings Settings => Singleton.GetNodeOrNull<Settings>("Settings");

    /// <summary>
    /// Node that maintains the FMOD Studio System. Main access point for the FMOD integration. <br/>Can be <c>null</c> if autoload hasn't initialized yet.
    /// </summary>
    public static FMODHandler FMOD => Singleton.GetNodeOrNull<FMODHandler>("FMOD");

    public Defluo()
    {
        DefluoLib.SetDllLoadDirectory();
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
    public static string Tr(string key, string context = "") =>
        TranslationServer.Translate(key, context);

    /// <summary>
    /// Returns the current locale's translation for the given key in plural form if possible
    /// </summary>
    /// <param name="key">String corresponding to the translation key in singular form</param>
    /// <param name="pluralKey">String corresponding to the translation key in plural form</param>
    /// <param name="number">Quantity of the plural object, used to fetch the correct form</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string Tr(string key, string pluralKey, int number, string context = "") =>
        TranslationServer.TranslatePlural(key, pluralKey, number, context);
}
