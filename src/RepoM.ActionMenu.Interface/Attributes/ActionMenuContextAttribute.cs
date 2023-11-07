namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class ActionMenuContextAttribute : Attribute
{
    public ActionMenuContextAttribute()
    {
        Alias = null!;
    }

    public ActionMenuContextAttribute(string alias)
    {
        Alias = alias;
    }

    public string? Alias { get; }
}