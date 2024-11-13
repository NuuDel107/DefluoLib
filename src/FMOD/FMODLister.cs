namespace DefluoLib;

using System;
using Godot;
using FMOD.Studio;
using System.Collections.Generic;
using System.IO;
#pragma warning disable IDE0005
using FMOD;
#pragma warning restore IDE0005

public class FMODLister
{
    public FMOD.Studio.System StudioSystem;

    public List<string> EventPaths = [];
    public List<string> BusPaths = [];
    public List<string> VCAPaths = [];
    public List<string> ParameterPaths = [];
    public List<Bank> Banks = [];

    public FMODLister()
    {
        // Initialize studio system
        FMOD.Studio.System.create(out StudioSystem);
        StudioSystem.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, 0);

        List();
    }

    private void List()
    {
        var bankFolder = ProjectSettings.GlobalizePath(
            ProjectSettings.GetSetting("DefluoLib/FMOD/BankFolder").As<string>()
        );

        // Load the master strings bank from the bank folder specified in settings
        var masterStringsPath = bankFolder + "/Master.strings.bank";
        FMODCaller.CheckResult(
            StudioSystem.loadBankFile(masterStringsPath, LOAD_BANK_FLAGS.NORMAL, out var stringBank)
        );

        // Load all other bank files in bank folder
        var bankFiles = Directory.GetFiles(bankFolder, "*.bank");
        foreach (var bankFile in bankFiles)
        {
            if (bankFile == Path.Combine(bankFolder, "Master.strings.bank"))
                continue;

            FMODCaller.CheckResult(
                StudioSystem.loadBankFile(bankFile, LOAD_BANK_FLAGS.NORMAL, out var bank)
            );
            Banks.Add(bank);
        }

        // Loop through all resource paths in bank and add them to lists
        // where they can be used by the editor properties
        stringBank.getStringCount(out var count);
        for (var i = 0; i < count; i++)
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
        Banks.Clear();
        List();
        Refreshed?.Invoke();
    }
}
