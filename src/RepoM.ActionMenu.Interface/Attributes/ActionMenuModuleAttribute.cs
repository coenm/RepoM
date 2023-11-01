namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class ActionMenuModuleAttribute : Attribute
{
    public ActionMenuModuleAttribute()
    {
        Alias = null!;
    }

    public ActionMenuModuleAttribute(string alias)
    {
        Alias = alias;
    }

    public string? Alias { get; }
}