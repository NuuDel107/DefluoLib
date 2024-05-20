using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DefluoLib;

/// <summary>
/// Base abstract class for a setting value
/// </summary>
public abstract class SettingBase
{
    private Variant _value;
    public Variant Value
    {
        get { return _value; }
        set
        {
            foreach (var function in subscribeFunctions)
                function(value);
            _value = value;
        }
    }

    public Variant DefaultValue;

    /// <summary>
    /// Name that is displayed in the settings UI
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// Optionally defined category for the setting
    /// </summary>
    public string Category;

    private List<Action<Variant>> subscribeFunctions = new();

    protected SettingBase(string displayName, Variant defaultValue)
    {
        DisplayName = displayName;
        Value = DefaultValue = defaultValue;
    }

    public void Subscribe(Action<Variant> function)
    {
        subscribeFunctions.Add(function);
        function(Value);
    }

    internal void SetCategory(string categoryName)
    {
        Category = categoryName;
    }
}

/// <summary>
/// A setting value that represents a boolean.
/// Define one as a property in a partial class declaration to register it to the settings.
/// </summary>
public class SettingBool : SettingBase
{
    /// <summary>
    /// Current value of the setting. Changing the value runs subscribed callback functions.
    /// </summary>
    new public bool Value
    {
        get { return (bool)base.Value; }
        set { base.Value = value; }
    }

    /// <summary>
    /// Default boolean value of the setting
    /// </summary>
    new public bool DefaultValue
    {
        get { return (bool)base.DefaultValue; }
        set { base.DefaultValue = value; }
    }

    /// <param name="displayName">Name that is displayed in the settings UI</param>
    /// <param name="defaultValue">Default boolean value of the setting</param>
    public SettingBool(string displayName, bool defaultValue)
        : base(displayName, defaultValue) { }

    /// <summary>
    /// Subscribes a callback to value changes of the setting
    /// </summary>
    /// <param name="function">Action that is ran when the value of the setting changes</param>
    public void Subscribe(Action<bool> function) =>
        Subscribe(new Action<Variant>(v => function((bool)v)));
}

/// <summary>
/// A setting value that represents a floating point number.
/// Define one as a property in a partial class declaration to register it to the settings.
/// </summary>
public class SettingFloat : SettingBase
{
    /// <summary>
    /// Constraints for acceptable number values
    /// </summary>
    public (float Min, float Max, float Step) Constraints;

    /// <summary>
    /// If true, setting is displayed as a percentage in the UI
    /// </summary>
    public bool IsPercentage;

    /// <summary>
    /// Current value of the setting. Changing the value runs subscribed callback functions.
    /// </summary>
    new public float Value
    {
        get { return (float)base.Value; }
        set
        {
            base.Value = Mathf.Clamp(
                Mathf.Round(value / Constraints.Step) * Constraints.Step,
                Constraints.Min,
                Constraints.Max
            );
        }
    }

    /// <summary>
    /// Default number value of the setting
    /// </summary>
    new public float DefaultValue
    {
        get { return (float)base.DefaultValue; }
        set { base.DefaultValue = value; }
    }

    /// <param name="displayName">Name that is displayed in the settings UI</param>
    /// <param name="defaultValue">Default number value of the setting</param>
    /// <param name="constraints">Constraints for acceptable number values</param>
    public SettingFloat(
        string displayName,
        float defaultValue,
        (float Min, float Max, float Step) constraints
    )
        : this(displayName, defaultValue, constraints, isPercentage: false) { }

    /// <param name="displayName">Name that is displayed in the settings UI</param>
    /// <param name="defaultValue">Default number value of the setting</param>
    /// <param name="constraints">Constraints for acceptable number values</param>
    /// <param name="isPercentage">If true, setting is displayed as a percentage in the UI</param>
    public SettingFloat(
        string displayName,
        float defaultValue,
        (float Min, float Max, float Step) constraints,
        bool isPercentage
    )
        : base(displayName, defaultValue)
    {
        Constraints = constraints;
        IsPercentage = isPercentage;
    }

    /// <summary>
    /// Subscribes a callback to value changes of the setting
    /// </summary>
    /// <param name="function">Action that is ran when the value of the setting changes</param>
    public void Subscribe(Action<float> function) =>
        Subscribe(new Action<Variant>(v => function((float)v)));
}

/// <summary>
/// A setting value that represents a string.
/// Define one as a property in a partial class declaration to register it to the settings.
/// </summary>
public class SettingString : SettingBase
{
    /// <summary>
    /// Possible string values for the setting. Used to populate a dropdown UI
    /// </summary>
    public string[] PossibleValues;

    /// <summary>
    /// Current value of the setting. Changing the value runs subscribed callback functions.
    /// </summary>
    new public string Value
    {
        get { return (string)base.Value; }
        set { base.Value = PossibleValues.Contains(value) ? value : PossibleValues[0]; }
    }

    /// <summary>
    /// Default string value of the setting
    /// </summary>
    new public string DefaultValue
    {
        get { return (string)base.DefaultValue; }
        set { base.DefaultValue = value; }
    }

    /// <param name="displayName">Name that is displayed in the settings UI</param>
    /// <param name="defaultValue">Default string value of the setting</param>
    /// <param name="possibleValues">Possible string values for the setting. Used to populate a dropdown UI</param>
    public SettingString(string displayName, string defaultValue, string[] possibleValues)
        : base(displayName, defaultValue)
    {
        PossibleValues = possibleValues;
    }

    /// <param name="displayName">Name that is displayed in the settings UI</param>
    /// <param name="defaultIndex">List index of the default string value</param>
    /// <param name="possibleValues">Possible string values for the setting. Used to populate a dropdown UI</param>
    public SettingString(string displayName, int defaultIndex, string[] possibleValues)
        : base(displayName, possibleValues[defaultIndex])
    {
        PossibleValues = possibleValues;
    }

    /// <summary>
    /// Subscribes a callback to value changes of the setting
    /// </summary>
    /// <param name="function">Action that is ran when the value of the setting changes</param>
    public void Subscribe(Action<string> function) =>
        Subscribe(new Action<Variant>(v => function((string)v)));
}

/// <summary>
/// Node that keeps track of settings and can load and save them.
/// </summary>
public partial class Settings : Node
{
    public const string UserSettingsResourcePath = "user://settings.tres";

    private List<PropertyInfo> properties;
    private DictionaryResource defaultResource;
    private DictionaryResource resource;

    public override void _EnterTree()
    {
        Name = "Settings";

        // Save defined properties to variables
        properties = GetSettingProperties();

        // Loop through keybinds and their properties to set categories for them
        // based on declared attributes
        var settings = GetSettings();
        CreateCategoryAttribute latestCategoryAttribute = null;
        foreach (var (setting, index) in settings.WithIndex())
        {
            var attribute = properties[index].GetCustomAttribute(typeof(CreateCategoryAttribute));
            if (attribute != null)
                latestCategoryAttribute = (CreateCategoryAttribute)attribute;
            if (latestCategoryAttribute != null)
                setting.SetCategory(latestCategoryAttribute.DisplayName);
        }

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
        Save();
    }

    /// <summary>
    /// Returns a list of defined setting properties
    /// </summary>
    /// <returns></returns>
    internal static List<PropertyInfo> GetSettingProperties()
    {
        IEnumerable<PropertyInfo> list =
            from property in typeof(Settings).GetProperties()
            where typeof(SettingBase).IsAssignableFrom(property.PropertyType)
            select property;

        return list.ToList();
    }

    /// <summary>
    /// Returns a list of defined settings
    /// </summary>
    public List<SettingBase> GetSettings()
    {
        List<SettingBase> list = properties
            .Select(property => (SettingBase)property.GetValue(this))
            .ToList();

        return list.ToList();
    }

    /// <summary>
    /// Creates a dictionary resource from current state of settings
    /// </summary>
    private DictionaryResource CreateResourceFromValues()
    {
        DictionaryResource newResource = new();

        // Loop through settings and create a dictionary entry for each of them
        foreach (var setting in GetSettings())
        {
            newResource.Dictionary.Add(setting.DisplayName, setting.Value);
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
        foreach (var setting in GetSettings())
        {
            if (resourceToLoad.Dictionary.ContainsKey(setting.DisplayName))
            {
                setting.Value = resourceToLoad.Dictionary[setting.DisplayName];
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
