using Godot;
using FMOD.Studio;

namespace DefluoLib;

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

    public Bank Bank;

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

    public void Unload()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.unload());
        IsLoaded = false;
        Bank = new Bank(0);
    }

    public void LoadSampleData()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.loadSampleData());
    }

    public void UnloadSampleData()
    {
        if (!IsLoaded)
            throw new System.Exception($"Bank {FilePath} hasn't been loaded");

        FMODCaller.CheckResult(Bank.unloadSampleData());
    }
}
