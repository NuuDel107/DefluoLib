using System;

namespace DefluoLib;

/// <summary>
/// Add this attribute to definitions to define categories for them
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CreateCategoryAttribute : Attribute
{
    public string DisplayName;

    public CreateCategoryAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}
