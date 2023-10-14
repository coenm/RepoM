namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class ActionMenuMemberAttribute : Attribute
{
    public ActionMenuMemberAttribute(string alias) : this(alias, string.Empty)
    {
    }

    public ActionMenuMemberAttribute(string alias, string category)
    {
        Alias = alias;
        Category = category;
    }

    public string Alias { get; }

    public string Category { get; }

    public bool Functor { get; set; }
}