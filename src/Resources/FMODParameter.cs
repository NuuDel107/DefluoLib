using Godot;
using FMOD.Studio;

namespace DefluoLib;

[Tool]
[GlobalClass]
public partial class FMODParameter : FMODResource
{
    public PARAMETER_DESCRIPTION ParameterDescription;

    public FMODParameter(string path)
        : base(path) { }

    public FMODParameter()
        : base() { }

    public string[] Labels;

    public float MinimumValue => ParameterDescription.minimum;
    public float MaximumValue => ParameterDescription.maximum;
    public float DefaultValue => ParameterDescription.defaultvalue;
    public bool IsReadonly => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.READONLY);
    public bool IsGlobal => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.GLOBAL);
    public bool IsDiscrete => ParameterDescription.flags.HasFlag(PARAMETER_FLAGS.DISCRETE);
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

    public void SetGlobally(float value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(
            Defluo.FMOD.StudioSystem.setParameterByName(Path, value, ignoreSeek)
        );
    }

    public void SetGlobally(string value, bool ignoreSeek = false)
    {
        FMODCaller.CheckResult(
            Defluo.FMOD.StudioSystem.setParameterByNameWithLabel(Path, value, ignoreSeek)
        );
    }

    public float GetGlobalValue()
    {
        FMODCaller.CheckResult(Defluo.FMOD.StudioSystem.getParameterByName(Path, out var value));
        return value;
    }
}
