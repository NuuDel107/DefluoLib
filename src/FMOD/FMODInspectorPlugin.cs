using Godot;

namespace DefluoLib;

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
        if (hintString.Length > 4 && hintString.Substring(0, 4) == "FMOD")
        {
            AddPropertyEditor(name, new FMODEditorProperty(hintString));
            return true;
        }
        return false;
    }
}
