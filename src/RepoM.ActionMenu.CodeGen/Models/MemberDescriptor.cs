namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public static class TypeInfoDescriptorFactory
{
    public static TypeInfoDescriptor Create(ITypeSymbol typeSymbol)
    {
        var displayString = typeSymbol.ToDisplayString();
        if (Program.TypeInfos.TryGetValue(displayString, out TypeInfoDescriptor? typeInfoDescriptor))
        {
            return typeInfoDescriptor;
        }

        var result = new TypeInfoDescriptor(typeSymbol);
        Program.TypeInfos.Add(displayString, result);
        return result;
    }
}

public class TypeInfoDescriptor
{
    public TypeInfoDescriptor(ITypeSymbol typeSymbol)
        : this (typeSymbol.Name, typeSymbol.ToDisplayString())
    {
        Nullable = IsNullableType(typeSymbol);

        if (Name.Contains("AutoCompleteOptionsV1"))
        {
            // do nothing intentionally, just for debugging coenm
        }
    }

    private static bool IsNullableType(ITypeSymbol typeSymbol)
    {
        return typeSymbol is INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated, };
    }

    public TypeInfoDescriptor(string name, string csharpTypeName)
    {
        CSharpTypeName = csharpTypeName;
        Name = name;

        if (CSharpTypeName.Contains("RepoM"))
        {
            // Name = CSharpTypeName.Split('.').Last();
            Name = name;
        }

        if (!csharpTypeName.Contains('.'))
        {
            // primitive?
            Name = CSharpTypeName;
        }

        if ("System.Collections.Generic.List<RepoM.ActionMenu.Interface.YamlModel.Templating.Text>".Equals(CSharpTypeName))
        {
            Name = "List<Text>";
        }
    }

    public string CSharpTypeName { get; }

    public string Name { get; }

    public string? Link { get; init; }

    public bool Nullable { get; set; } = false;
}

/// <summary>
/// Property, Function, field etc. etc.
/// </summary>
public class MemberDescriptor : IXmlDocsExtended
{
    /// <summary>
    /// Friendly Name
    /// </summary>
    public string Name { get; init; } = null!;

    public string CSharpName { get; set; } = null!;

    public TypeInfoDescriptor ReturnType { get; set; }

    public string XmlId { get; set; } = null!;

    public bool IsCommand { get; set; }

    public bool IsAction { get; set; }

    public bool IsFunc { get; set; }

    public bool IsConst { get; set; }

    /// <remarks>
    /// Used for C# code generation
    /// </remarks>
    public string? Cast { get; set; }
    
    public string Description { get; set; } = null!;

    public string? InheritDocs { get; set; }

    public string Returns { get; set; } = null!;

    public string Remarks { get; set; } = null!;

    public ExamplesDescriptor? Examples { get; set; }

    public List<ParamDescriptor> Params { get; } = [];
}

public class ActionMenuMemberDescriptor : MemberDescriptor
{
    // public RepositoryActionAttribute RepositoryActionAttribute { get; init; }

    public bool IsTemplate { get; set; } = false;

    public bool IsPredicate { get; set; } = false;

    public bool IsContext { get; set; } = false;

    public object DefaultValue { get; set; } = null!;

    public bool IsReturnEnumerable { get; set; } = false;

    public string? RefType { get; set; } = null;
}

public class ActionMenuContextMemberDescriptor : MemberDescriptor
{
    public string ActionMenuContextMemberName => Name;
}
