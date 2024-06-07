using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// FMOD bus asset that can be used to control bus properties
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODBus : FMODAsset
{
    public Resource BusAsset
    {
        get { return Asset; }
        set { Asset = value; }
    }

    private FMODBusInstance _bus;
    public FMODBusInstance Bus
    {
        get
        {
            if (_bus == null && Defluo.FMOD.IsNodeReady())
            {
                _bus = Defluo.FMOD.GetBus(this);
            }
            return _bus;
        }
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "BusAsset" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "BusAsset" }
            }
        };
        return properties;
    }

    /// <summary>
    /// Sets volume of bus
    /// </summary>
    public void SetVolume(float volume)
    {
        Bus.SetVolume(volume);
    }

    /// <summary>
    /// Pauses or unpauses bus
    /// </summary>
    public void SetPaused(bool paused)
    {
        Bus.SetPaused(paused);
    }
}
