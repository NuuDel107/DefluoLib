namespace DefluoLib;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Miscellaneous extensions to pre-existing C# and Godot classes
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Returns an <c>IEnumerable</c> with the index added to the sequence as a tuple,
    /// useful for getting index inside a foreach loop
    /// </summary>
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((item, index) => (item, index));

    /// <summary>
    /// Runs given predicate for each value in a <c>Vector3</c>
    /// </summary>
    public static Vector3 Select(this Vector3 self, Func<float, float> predicate)
    {
        self.X = predicate(self.X);
        self.Y = predicate(self.Y);
        self.Z = predicate(self.Z);
        return self;
    }

    /// <summary>
    /// Rounds <c>Vector3</c> down to specified digits
    /// </summary>
    public static Vector3 Round(this Vector3 self, int digits = 0) =>
        self.Select(value => (float)Math.Round((double)value, digits));

    /// <summary>
    /// Converts <c>Vector3</c> to a struct used in FMOD API calls
    /// </summary>
    public static FMOD.VECTOR ToFMOD(this Vector3 self) =>
        new FMOD.VECTOR() with
        {
            x = -self.X,
            y = self.Y,
            z = self.Z
        };

    /// <summary>
    /// Converts FMOD Core API's <c>VECTOR</c> struct to Godot's <c>Vector3</c>
    /// </summary>
    public static Vector3 ToGodot(this FMOD.VECTOR self) => new(-self.x, self.y, self.z);

    /// <summary>
    /// Returns the first instance of a child node of given type
    /// </summary>
    /// <typeparam name="T">Wanted type of child node</typeparam>
    /// <returns></returns>
    public static T GetChildByType<T>(this Node self)
        where T : Node
    {
        foreach (var child in self.GetChildren())
        {
            if (child is T correctChild)
                return correctChild;
        }
        throw new ArgumentException($"No child of given type was found");
    }

    public static List<Node> GetDescendants(this Node self)
    {
        List<Node> nodes = [];
        foreach (var child in self.GetChildren())
        {
            nodes.Add(child);
            if (child.GetChildCount() > 0)
                nodes.AddRange(child.GetDescendants());
        }
        return nodes;
    }
}
