namespace RepoM.ActionMenu.Interface.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class ActionMenuMemberAttribute : Attribute
{
    public ActionMenuMemberAttribute(string alias)
    {
        Alias = alias;
    }

    public string Alias { get; }
    
    public bool Functor { get; set; }
}