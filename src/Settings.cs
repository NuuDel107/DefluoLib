using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DefluoLib;

/// <summary>
/// A setting value representing a variant.
/// </summary>
public class VariantSetting
{
    private Variant _value;
    public Variant Value
    {
        get { return _value; }
        set
        {
            ValueChanged?.Invoke(value);
            _value = value;
        }
    }

    public Godot.Variant.Type Type = Variant.Type.Nil;
    public Variant DefaultValue;

    public VariantSetting(Variant defaultValue)
    {
        Value = DefaultValue = defaultValue;
    }

    public event Action<Variant> ValueChanged;
}

/// <summary>
/// A setting value representing a boolean.
/// </summary>
public class BoolSetting : VariantSetting
{
    public BoolSetting(bool defaultValue)
        : base(defaultValue)
    {
        Type = Variant.Type.Bool;
        base.ValueChanged += value => ValueChanged?.Invoke(value.As<bool>());
    }

    public new event Action<bool> ValueChanged;
}

/// <summary>
/// A setting value representing a float.
/// </summary>
public class FloatSetting : VariantSetting
{
    public FloatSetting(float defaultValue)
        : base(defaultValue)
    {
        Type = Variant.Type.Float;
        base.ValueChanged += value => ValueChanged?.Invoke(value.As<float>());
    }

    public new event Action<float> ValueChanged;
}

/// <summary>
/// A setting value representing an enum.
/// </summary>
public class EnumSetting<T> : VariantSetting
    where T : System.Enum
{
    public EnumSetting(int defaultValue)
        : base(defaultValue)
    {
        Type = Variant.Type.Int;
        base.ValueChanged += value => ValueChanged?.Invoke(value.As<T>());
    }

    public new event Action<T> ValueChanged;
}

/// <summary>
/// A setting value representing a string.
/// </summary>
public class StringSetting : VariantSetting
{
    public StringSetting(string defaultValue)
        : base(defaultValue)
    {
        Type = Variant.Type.String;
        base.ValueChanged += value => ValueChanged?.Invoke(value.As<string>());
    }

    public new event Action<string> ValueChanged;
}

/// <summary>
/// Node that keeps track of settings and can load and save them.
/// </summary>
public partial class Settings : Node
{
    public const string UserSettingsResourcePath = "user://settings.tres";

    private List<PropertyInfo> settingProperties;
    private DictionaryResource defaultResource;
    private DictionaryResource resource;

    public override void _EnterTree()
    {
        Name = "Settings";

        // Save defined properties to list
        settingProperties = GetSettingProperties();

        // Create a default settings resource by running resource creation function
        // using settings with default values before they get overwritten by initialization
        defaultResource = CreateResourceFromValues();

        if (ResourceLoader.Exists(UserSettingsResourcePath))
        {
            resource = ResourceLoader.Load<DictionaryResource>(UserSettingsResourcePath);
            Load(resource);
        }
        else
        {
            GD.Print("User settings not found, using default values");
            resource = defaultResource;
        }
    }

    /// <summary>
    /// Returns a list of defined setting properties
    /// </summary>
    /// <returns></returns>
    internal static List<PropertyInfo> GetSettingProperties()
    {
        List<PropertyInfo> list = new();
        foreach (var property in typeof(Settings).GetProperties())
        {
            if (typeof(VariantSetting).IsAssignableFrom(property.PropertyType))
                list.Add(property);
        }

        return list;
    }

    /// <summary>
    /// Returns a dictionary of defined settings, with the keys being their names
    /// </summary>
    public Dictionary<string, VariantSetting> GetSettings()
    {
        Dictionary<string, VariantSetting> settings = new();
        foreach (var property in settingProperties)
        {
            settings.Add(property.Name, (VariantSetting)property.GetValue(this));
        }
        return settings;
    }

    /// <summary>
    /// Creates a dictionary resource from current state of settings
    /// </summary>
    private DictionaryResource CreateResourceFromValues()
    {
        DictionaryResource newResource = new();

        // Loop through settings and create a dictionary entry for each of them
        foreach (var (settingName, setting) in GetSettings())
        {
            newResource.Dictionary.Add(settingName, (Variant)setting.Value);
        }
        return newResource;
    }

    /// <summary>
    /// Sets state of settings according to given resource dictionary
    /// </summary>
    private void Load(DictionaryResource resourceToLoad)
    {
        // Loop through settings and set value of setting
        // if definition name is found in resource dictionary
        foreach (var (settingName, setting) in GetSettings())
        {
            if (resourceToLoad.Dictionary.ContainsKey(settingName))
            {
                setting.Value = resourceToLoad.Dictionary[settingName];
            }
        }
        Defluo.Print("Loaded settings from resource");
    }

    /// <summary>
    /// Saves current setting values to disk as a resource file
    /// </summary>
    public void Save()
    {
        resource = CreateResourceFromValues();
        ResourceSaver.Save(resource, UserSettingsResourcePath);
        Defluo.Print("Saved settings to disk");
    }

    /// <summary>
    /// Resets all settings to their default values
    /// </summary>
    public void Reset()
    {
        resource = defaultResource;
        Load(resource);
        SettingsReset.Invoke();
        Defluo.Print("Reset settings to their default values");
    }

    /// <summary>
    /// Invoked when settings are reset
    /// </summary>
    public event Action SettingsReset;

    public override void _ExitTree()
    {
        Save();
    }
}
