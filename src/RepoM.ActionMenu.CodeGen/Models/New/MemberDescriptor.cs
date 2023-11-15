namespace RepoM.ActionMenu.CodeGen.Models.New;

using System;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Property, Function, field etc. etc.
/// </summary>
public class MemberDescriptor
{
    public string CSharpName { get; set; }

    public string ReturnType { get; set; }

    public string XmlId { get; set; }
}

public class ActionMenuMemberDescriptor : MemberDescriptor
{
    // public RepositoryActionAttribute RepositoryActionAttribute { get; init; }
    
}

public class ActionMenuContextMemberDescriptor : MemberDescriptor
{
    public ActionMenuContextMemberAttribute ActionMenuContextMemberAttribute { get; set; }


    public bool IsCommand { get; set; }

    public bool IsAction { get; set; }

    public bool IsFunc { get; set; }

    public bool IsConst { get; set; }
    public string Cast { get; set; }

}
