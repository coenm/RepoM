namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.Attributes;

/// <summary>
/// Property, Function, field etc. etc.
/// </summary>
public class MemberDescriptor : IXmlDocsExtended
{
    public string CSharpName { get; set; }

    public string ReturnType { get; set; }

    public string XmlId { get; set; }


    public bool IsCommand { get; set; }

    public bool IsAction { get; set; }

    public bool IsFunc { get; set; }

    public bool IsConst { get; set; }

    public string Cast { get; set; }




    public string Description { get; set; }
    public string Returns { get; set; }
    public string Remarks { get; set; }
    public ExamplesDescriptor? Examples { get; set; }
    public List<ParamDescriptor> Params { get; } = new List<ParamDescriptor>();
}

public class ActionMenuMemberDescriptor : MemberDescriptor
{
    // public RepositoryActionAttribute RepositoryActionAttribute { get; init; }
    
}

public class ActionMenuContextMemberDescriptor : MemberDescriptor
{
    public string ActionMenuContextMemberName { get; set; }
}
