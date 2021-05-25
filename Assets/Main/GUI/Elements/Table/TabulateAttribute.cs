using System;

/// <summary>
/// Flags fields and properties to be avaiable to GUI tables.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TabulateAttribute : Attribute
{
    public string columnName;

    public TabulateAttribute(string columnName)
    {
        this.columnName = columnName;
    }
}
