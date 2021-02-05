using System;

/// <summary>
/// Flags fields and properties to be avaiable to GUI tables.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GUITableValueAttribute : Attribute
{
    public string columnName;

    public GUITableValueAttribute(string columnName)
    {
        this.columnName = columnName;
    }
}
