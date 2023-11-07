namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class ActionMenuContextMemberAttribute : Attribute
{
    public ActionMenuContextMemberAttribute(string alias)
    {
        Alias = alias;
    }

    public string Alias { get; }
}