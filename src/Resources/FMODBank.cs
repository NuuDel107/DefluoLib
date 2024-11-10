using Godot;
using FMOD.Studio;

namespace DefluoLib;

/// <summary>
/// A <see href="https://www.fmod.com/docs/2.03/studio/getting-events-into-your-game.html#banks">Bank</see> resource.
/// Can be exported from a node to set the path to the bank file in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODBank : Resource
{
    [Export(PropertyHint.File, "*.bank")]
    public string FilePath { get; set; }

    private string path;
    public string Path
    {
        get => path;
        set
        {
            path = value;
            if (Defluo.FMOD.IsStudioSystemInitialized)
                Init();
            else
                Defluo.FMOD.StudioSystemInitialized += Init;
        }
    }

    /// <summary>
    /// The raw API <see href="https://www.fmod.com/docs/2.03/api/studio-api-bank.html">Bank</see> object.
    /// </summary>
    public Bank Bank;

    /// <summary>
    /// If bank is loaded into the studio system.
    /// </summary>
    public bool IsLoaded;

    public FMODBank(string path, bool isLoaded = false)
    {
        IsLoaded = isLoaded;
        if (IsLoaded)
            Path = path;
        else
            FilePath = path;
    }

    public FMODBank() { }

    protected void Init()
    {
        if (!IsLoaded)
            return;

        if (!FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getBank(FilePath, out Bank)))
            throw new System.ArgumentException($"Invalid loaded bank path {FilePath}");
    }

    /// <summary>
    /// Loads the bank into the studio system.
    /// </summary>
    /// <param name="loadFlags"></param>
    public void Load(LOAD_BANK_FLAGS loadFlags = LOAD_BANK_FLAGS.NORMAL)
    {
        if (IsLoaded)
            throw new System.Exception($"Bank {FilePath} is already loaded");

        if (
            !FMODCaller.CheckResult(
                Defluo.FMOD.StudioSystem.loadBankFile(
                    ProjectSettings.GlobalizePath(FilePath),
                    loadFlags,
                    out Bank
                )
            )
        )
        {
            throw new System.ArgumentException($"Invalid bank file path {FilePath}");
        }
        IsLoaded = true;
        Bank.getPath(out path);
    }

    /// <summary>
    /// Unloads the bank from the studio system.
    /// </summary>
    public void Unload()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.unload());
        IsLoaded = false;
        Bank = new Bank(0);
    }

    /// <summary>
    /// Loads all of the sample data (sound assets) contained in the bank into memory.
    /// </summary>
    public void LoadSampleData()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.loadSampleData());
    }

    /// <summary>
    /// Unloads the bank's sample data.
    /// </summary>
    public void UnloadSampleData()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.unloadSampleData());
    }
}
