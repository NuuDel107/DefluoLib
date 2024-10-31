using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace DefluoLib;

/// <summary>
/// FMOD parameter that can be used to change parameters locally or globally
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODParameter : FMODAsset
{
    public Resource ParameterAsset
    {
        get { return Asset; }
        set { Asset = value; }
    }

    [Flags]
    private enum ParameterFlags
    {
        Readonly = 1,
        Automatic = 1 << 1,
        Global = 1 << 2,
        Discrete = 1 << 3,
        Labeled = 1 << 4,
    }

    private Resource parameterDescription => (Resource)ParameterAsset.Get("parameter_description");
    private ParameterFlags flags => (ParameterFlags)(int)parameterDescription.Get("flags");

    /// <summary>
    /// Defined minimum value that parameter can be set to
    /// </summary>
    public float MinimumValue => (float)parameterDescription.Get("minimum");

    /// <summary>
    /// Defined maximum value that parameter can be set to
    /// </summary>
    public float MaximumValue => (float)parameterDescription.Get("maximum");

    /// <summary>
    /// Value that parameter is set to by default
    /// /// </summary>
    public float DefaultValue => (float)parameterDescription.Get("default_value");

    /// <summary>
    /// If parameter is read only
    /// </summary>
    public bool IsReadonly => flags.HasFlag(ParameterFlags.Readonly);

    /// <summary>
    /// If parameter is set in global scope.
    /// Otherwise parameter must be set individually for every event instance (a.k.a. in local scope)
    /// </summary>
    public bool IsGlobal => flags.HasFlag(ParameterFlags.Global);

    /// <summary>
    /// If true, parameter only accepts integers a.k.a. whole numbers for its values
    /// </summary>
    public bool IsDiscrete => flags.HasFlag(ParameterFlags.Discrete);

    /// <summary>
    /// If true, parameter values can be set using its defined labels. They can be found in the Labels property
    /// </summary>
    public bool IsLabeled => flags.HasFlag(ParameterFlags.Labeled);

    /// <summary>
    /// List of possible labels that can be used to set the value of this parameter
    /// </summary>
    public string[] Labels => ParameterAsset.Get("labels").As<Array<string>>().ToArray();

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "ParameterAsset" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "ParameterAsset" }
            }
        };
        return properties;
    }

    /// <summary>
    /// Sets the value of this parameter in the global scope.
    /// </summary>
    /// <param name="value">Value to set parameter to</param>
    public void SetGlobally(float value) => Defluo.FMOD.SetGlobalParameter(this, value);

    /// <summary>
    /// Sets the value of this parameter in the global scope.
    /// </summary>
    /// <param name="value">Labelled value to set parameter to</param>
    public void SetGlobally(string value) => Defluo.FMOD.SetGlobalParameter(this, value);
}
