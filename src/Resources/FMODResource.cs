using Godot;

namespace DefluoLib;

[Tool]
[GlobalClass]
public abstract partial class FMODResource : Resource
{
    private string path;

    /// <summary>
    /// The local path of an FMOD resource.
    /// </summary>
    [Export]
    public string Path
    {
        get => path;
        set
        {
            path = value;

            if (Engine.IsEditorHint())
                return;

            if (Defluo.FMOD.IsStudioSystemInitialized)
                Init();
            else
                Defluo.FMOD.StudioSystemInitialized += Init;
        }
    }

    public FMODResource(string path)
    {
        Path = path;
    }

    public FMODResource() { }

    protected virtual void Init() { }
}
