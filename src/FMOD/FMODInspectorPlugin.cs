namespace DefluoLib;

using Godot;

[Tool]
public partial class FMODInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object)
    {
        // We support all objects in this example.
        return true;
    }

    public FMODInspectorPlugin() { }

    public override bool _ParseProperty(
        GodotObject @object,
        Variant.Type type,
        string name,
        PropertyHint hintType,
        string hintString,
        PropertyUsageFlags usageFlags,
        bool wide
    )
    {
        // If the property class name starts with FMOD,
        // it's (hopefully) an exported FMODResource
        // Event playback resources are created with the default inspector
        if (hintString.Length > 4 && hintString[..4] == "FMOD" && hintString != "FMODEventPlayback")
        {
            AddPropertyEditor(name, new FMODEditorProperty(hintString));
            return true;
        }
        return false;
    }
}
