using Godot;
using FMOD.Studio;

namespace DefluoLib;

/// <summary>
/// A <see href="https://www.fmod.com/docs/2.03/studio/parameters.html">parameter</see> resource.
/// Can be exported from a node to be selected in the editor.
/// </summary>
[Tool]
[GlobalClass]
public partial class FMODParameter : FMODResource
{
    public PARAMETER_DESCRIPTION ParameterDescription;

    public FMODParameter(string path)
        : base(path) { }

    public FMODParameter()
        : base() { }

    /// <summary>
    /// Possible string values that can be used to set the parameter's value.
    /// Is empty for non-labeled parameters.
    /// </summary>
    public string[] Labels;

    /// <summary>
    /// The lowest value that the parameter accepts as its value.
    /// </summary>
    public float MinimumValue => ParameterDescription.minimum;

    /// <summary>
    /// The highest value that the parameter accepts as its value.
    /// </summary>
    public float MaximumValue => ParameterDescription.maximum;

    /// <summary>
    /// The default value of the parameter.
    /// </summary>
    public float DefaultValue => ParameterDescription.defaultvalue;

    /// <summary>
    /// If parameter is read only.
    /// </summary>
    public bool IsReadonly => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.READONLY);

    /// <summary>
    /// If parameter is global, meaning all event instances share the same value.
    /// /// Otherwise the parameter is local, meaning it needs to be set individually for all event instances.
    /// </summary>
    public bool IsGlobal => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.GLOBAL);

    /// <summary>
    /// If parameter only accepts integers (whole numbers) as its values
    /// </summary>
    public bool IsDiscrete => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.DISCRETE);

    /// <summary>
    /// If string values can be used to set the parameter's value.
    /// Check <c>Labels</c> for possible values.
    /// </summary>
    /// <returns></returns>
    public bool IsLabeled => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.LABELED);

    protected override void Init()
    {
        if (
            !FMODCaller.CheckResult(
                Defluo.FMOD.StudioSystem.getParameterDescriptionByName(
                    Path,
                    out ParameterDescription
                )
            )
        )
            throw new System.ArgumentException($"Invalid parameter path {Path}");

        if (!IsLabeled)
            return;

        Labels = new string[(int)MaximumValue];
        for (int i = 0; i < MaximumValue; i++)
        {
            FMODCaller.CheckResult(
                Defluo.FMOD.StudioSystem.getParameterLabelByName(Path, i, out var label)
            );
            Labels[i] = label;
        }
    }

    /// <summary>
    /// Sets the value of the parameter in the global scope.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetGlobally(float value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(
            Defluo.FMOD.StudioSystem.setParameterByName(Path, value, ignoreSeek)
        );
    }

    /// <summary>
    /// Sets the value of the parameter in the global scope.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="ignoreSeek">Whether to ignore the parameter's seek speed and set the value immediately</param>
    public void SetGlobally(string value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(
            Defluo.FMOD.StudioSystem.setParameterByNameWithLabel(Path, value, ignoreSeek)
        );
    }

    /// <summary>
    /// Returns the current value of parameter in the global scope.
    /// </summary>
    public float GetGlobalValue()
    {
        FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getParameterByName(Path, out var value));
        return value;
    }

    /// <summary>
    /// Returns the current value of parameter in the global scope as a string label
    /// </summary>
    public string GetGlobalValueAsLabel()
    {
        if (!IsLabeled)
            throw new System.Exception("Parameter is not labeled");

        return Labels[(int)GetGlobalValue()];
    }
}
