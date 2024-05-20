using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// FMOD VCA asset that can be used to control VCA properties
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODVCA : FMODAsset
{
    public Resource VCAAsset
    {
        get { return Asset; }
        set { Asset = value; }
    }

    public FMODVCAInstance VCA;

    public FMODVCA()
    {
        if (Engine.IsEditorHint())
            return;

        Defluo.Singleton.Ready += () => VCA = Defluo.FMOD.GetVCA(this);
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "VCAAsset" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "VCAAsset" }
            }
        };
        return properties;
    }

    /// <summary>
    /// Sets volume of VCA
    /// </summary>
    public void SetVolume(float volume)
    {
        VCA.SetVolume(volume);
    }
}
