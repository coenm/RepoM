namespace RepoM.ActionMenu.CodeGen.Models.New;

using System;
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