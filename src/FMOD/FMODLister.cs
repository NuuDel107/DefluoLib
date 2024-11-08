namespace DefluoLib;

using System;
using System.Runtime.InteropServices;

using Godot;
using FMOD;
using FMOD.Studio;
using System.Collections.Generic;

public class FMODLister
{
    public FMOD.Studio.System StudioSystem;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool SetDllDirectory(string lpPathName);

    public List<string> EventPaths = new();
    public List<string> BusPaths = new();
    public List<string> VCAPaths = new();
    public List<string> ParameterPaths = new();

    public FMODLister()
    {
        // Set the default directory for importing DLLs
        SetDllDirectory(ProjectSettings.GlobalizePath("res://addons/DefluoLib/bin"));

        // Initialize studio system
        FMOD.Studio.System.create(out StudioSystem);
        StudioSystem.initialize(
            512,
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            (IntPtr)0
        );

        List();
    }

    private void List()
    {
        // Load the master strings bank from the bank folder specified in settings
        var masterStringsPath = ProjectSettings.GlobalizePath(
            ProjectSettings.GetSetting("DefluoLib/FMOD/BankFolder").As<string>()
                + "/Master.strings.bank"
        );
        FMODCaller.CheckResult(
            StudioSystem.loadBankFile(masterStringsPath, LOAD_BANK_FLAGS.NORMAL, out var stringBank)
        );

        // Loop through all resource paths in bank and add them to lists
        // where they can be used by the editor properties
        stringBank.getStringCount(out var count);
        for (int i = 0; i < count; i++)
        {
            stringBank.getStringInfo(i, out var guid, out var path);

            var type = path.Split("/")[0];
            switch (type)
            {
                case "event:":
                    EventPaths.Add(path);
                    break;
                case "bus:":
                    BusPaths.Add(path);
                    break;
                case "vca:":
                    VCAPaths.Add(path);
                    break;
                case "parameter:":
                    ParameterPaths.Add(path);
                    break;
            }
        }
    }

    public event Action Refreshed;

    public void Refresh()
    {
        FMODCaller.CheckResult(StudioSystem.unloadAll());
        EventPaths.Clear();
        BusPaths.Clear();
        VCAPaths.Clear();
        ParameterPaths.Clear();
        List();
        Refreshed?.Invoke();
    }
}
