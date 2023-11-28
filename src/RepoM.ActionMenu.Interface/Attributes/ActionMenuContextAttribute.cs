namespace RepoM.ActionMenu.Interface.Attributes;

using System;

/// <summary>
/// ActionMenuContext is Scriban context (i.e. methods, variables and contants).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ActionMenuContextAttribute : Attribute
{
    public ActionMenuContextAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}