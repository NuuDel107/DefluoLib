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

        FMOD.Studio.System.create(out StudioSystem);
        StudioSystem.initialize(
            512,
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            (IntPtr)0
        );

        var masterStringsPath = ProjectSettings.GlobalizePath(
            ProjectSettings.GetSetting("DefluoLib/FMOD/BankFolder").As<string>()
                + "/Master.strings.bank"
        );
        StudioSystem.loadBankFile(masterStringsPath, LOAD_BANK_FLAGS.NORMAL, out var stringBank);

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
}
