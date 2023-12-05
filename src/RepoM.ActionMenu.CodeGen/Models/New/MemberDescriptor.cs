namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.Interface.Attributes;

/// <summary>
/// Property, Function, field etc. etc.
/// </summary>
public class MemberDescriptor : IXmlDocsExtended
{
    /// <summary>
    /// Friendly Name
    /// </summary>
    public string Name { get; set; }

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

    public bool IsTemplate { get; set; } = false;

    public bool IsPredicate { get; set; } = false;

    public bool IsContext { get; set; } = false;

    public object DefaultValue { get; set; } = null;

    public bool IsReturnEnumerable { get; set; } = false;

    public string? RefType { get; set; } = null;
}

public class ActionMenuContextMemberDescriptor : MemberDescriptor
{
    public string ActionMenuContextMemberName => Name;
}
