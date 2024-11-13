namespace DefluoLib;

using Godot;

/// <summary>
/// Class that contains static string formatting functions.
/// </summary>
public partial class StringFormat
{
    /// <summary>
    /// Sets the color of a given string with BBCode
    /// </summary>
    /// <param name="str">String to apply color to<</param>
    /// <param name="color">Godot enum color</param>
    public static string Colored(string str, Color color) => Colored(str, color.ToHtml(false));

    /// <summary>
    /// Sets the color of a given string with BBCode
    /// </summary>
    /// <param name="str">String to apply color to</param>
    /// <param name="color">Color in hexcode format (<c>#ffffff</c>)</param>
    public static string Colored(string str, string color) => $"[color=#{color}]{str}[/color]";

    /// <summary>
    /// Converts naming conventions into pascalCase.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string SnakeToPascal(string str)
    {
        var output = "";
        var words = str.Split('_');
        words[0] = words[0][0].ToString().ToLowerInvariant() + words[0][1..];

        if (words.Length == 1)
            return words[0];
        for (var i = 0; i < words.Length; i++)
        {
            if (i != 0)
                words[i] =
                    words[i][0].ToString().ToUpperInvariant() + words[i][1..].ToLowerInvariant();
            output += words[i];
        }
        return output;
    }

    /// <summary>
    /// Rounds a float to a specified number of decimals
    /// </summary>
    /// <param name="number"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public static string FloatRounded(float number, int decimals = 4)
    {
        return number.ToString($"n{decimals}");
    }

    /// <summary>
    /// Converts a variant into a string (with style)
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public static string VariantToString(Variant variant)
    {
        var output = "";
        switch (variant.VariantType)
        {
            case Variant.Type.Bool:
            {
                var value = (bool)variant;
                output = value ? Colored("true", Colors.Green) : Colored("false", Colors.Red);
                break;
            }

            case Variant.Type.Float:
            {
                var value = (float)variant;
                output = FloatRounded(value);
                break;
            }

            case Variant.Type.Vector2:
            {
                var value = (Vector2)variant;
                output =
                    $"({Colored(FloatRounded(value.X), Colors.Red)}, {Colored(FloatRounded(value.Y), Colors.Green)})";
                break;
            }

            case Variant.Type.Vector2I:
            {
                var value = (Vector2I)variant;
                output =
                    $"({Colored(value.X.ToString(), Colors.Red)}, {Colored(value.Y.ToString(), Colors.Green)})";
                break;
            }

            case Variant.Type.Vector3:
            {
                var value = (Vector3)variant;
                output =
                    $"({Colored(FloatRounded(value.X), Colors.Red)}, {Colored(FloatRounded(value.Y), Colors.Green)}, {Colored(FloatRounded(value.Z), Colors.Blue)})";
                break;
            }

            case Variant.Type.Basis:
            {
                var value = (Basis)variant;
                output =
                    $"{Colored("X:", Colors.Red)} {VariantToString(value.X)}, {Colored("Y:", Colors.Green)} {VariantToString(value.Y)}, {Colored("Z:", Colors.Blue)} {VariantToString(value.Z)}";
                break;
            }

            case Variant.Type.Array:
            {
                var array = (Godot.Collections.Array)variant;
                output += "[";

                foreach (var item in array)
                {
                    output += $"{VariantToString(item)}, ";
                }

                if (output.Length > 2)
                    output = output[..^2];
                output += "]";
                break;
            }

            case Variant.Type.Dictionary:
            {
                var dict = (Godot.Collections.Dictionary)variant;
                output += "{\n";

                foreach (var (key, value) in dict)
                {
                    output += $"\"{VariantToString(key)}\": {VariantToString(value)},\n";
                }

                if (output.Length > 2)
                    output = output[..^2];
                output += "}\n";
                break;
            }

            case Variant.Type.Object:
            {
                var type = ((GodotObject)variant).GetType();
                if (type == typeof(AnimationTree))
                {
                    var value = (AnimationTree)variant;
                    var properties = value.GetPropertyList();
                    foreach (var property in properties)
                    {
                        var name = (string)property["name"];
                        if (name.StartsWith("parameters"))
                            output += $"{name[11..]}: {FloatRounded((float)value.Get(name))} ";
                    }
                }
                else if (type == typeof(NavigationAgent3D))
                {
                    var value = (NavigationAgent3D)variant;
                    output =
                        $"isTargetReachable: {VariantToString(value.IsTargetReachable())} isTargetReached: {VariantToString(value.IsTargetReached())} finalPosition: {VariantToString(value.GetFinalPosition())}";
                }
                else if (type == typeof(Timer))
                {
                    var value = (Timer)variant;
                    output =
                        $"timeLeft: {VariantToString(value.TimeLeft)} isStopped: {VariantToString(value.IsStopped())}";
                }
                else
                    output = variant.ToString();
                break;
            }

            default:
            {
                output = variant.ToString();
                break;
            }
        }
        return Colored(output, Colors.White);
    }
}
