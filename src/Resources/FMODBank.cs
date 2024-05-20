using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// FMOD bank asset that can be instantiated and loaded into memory
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODBank : FMODAsset
{
    public Resource BankAsset
    {
        get { return Asset; }
        set { Asset = value; }
    }

    public FMODBankInstance Bank;

    /// <summary>
    /// File path of the bank within the project file structure
    /// </summary>
    public string FilePath
    {
        get { return (string)BankAsset.Get("file_path"); }
    }

    /// <summary>
    /// Time when bank was last built in Unix time
    /// </summary>
    public long ModifiedTime
    {
        get { return (long)BankAsset.Get("modified_time"); }
    }

    public FMODBank()
    {
        if (Engine.IsEditorHint())
            return;

        Defluo.Singleton.Ready += () => Bank = Defluo.FMOD.GetBank(this);
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "BankAsset" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "BankAsset" }
            }
        };
        return properties;
    }
}
