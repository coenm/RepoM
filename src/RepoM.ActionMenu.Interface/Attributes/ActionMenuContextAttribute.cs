namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class ActionMenuContextAttribute : Attribute
{
    public ActionMenuContextAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}