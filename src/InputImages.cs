using Godot;

namespace DefluoLib;

public partial class Input : Node
{
    public static Texture2D GetInputTexture(DigitalInput input)
    {
        string folderPath = "";
        switch (input.Type)
        {
            case DigitalInputType.Key:
                folderPath = (string)
                    ProjectSettings.Singleton.GetSetting("DefluoLib/Input/KeyboardIconFolder");
                break;
            case DigitalInputType.MouseButton:
                folderPath = (string)ProjectSettings.GetSetting("DefluoLib/Input/MouseIconFolder");
                break;
            case DigitalInputType.JoyButton:
            case DigitalInputType.JoyAxis:
                folderPath = (string)ProjectSettings.GetSetting("DefluoLib/Input/XboxIconFolder");
                break;
        }
        var path = $"{folderPath}/{input.DisplayString}.png";
        if (ResourceLoader.Exists(path))
            return ResourceLoader.Load<Texture2D>(path);
        else
        {
            GD.PushError($"Input texture couldn't be found at path {path}");
            return null;
        }
    }
}
