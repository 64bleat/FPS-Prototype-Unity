using System;

public enum GUIValidation { Available, Unavailable, Hidden }

/// <summary>
/// Flags methods to be available in context menus.
/// </summary>
/// <remarks> Context menus can open prompts in order to fill parameters of non-void methods. </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class ContextMenuAttribute : Attribute
{
    public string name;

    public ContextMenuAttribute(string name)
    {
        this.name = name;
    }
}
