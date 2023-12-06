namespace RepoM.ActionMenu.CodeGen.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RepoM.ActionMenu.CodeGen;

public enum SymbolType
{
    Class,

    Enum,
}

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ClassDescriptor : IXmlDocsExtended
{
    private SymbolType _symbolType = SymbolType.Class;

    /// <summary>
    /// Properties, Functions, fields etc. etc.
    /// </summary>
    public List<MemberDescriptor> Members { get; set; } = new List<MemberDescriptor>();

    /// <summary>
    /// Friendly name
    /// </summary>
    public string Name { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public string Namespace { get; set; } = null!;

    public bool IsEnum => _symbolType == SymbolType.Enum;

    public bool IsClass => _symbolType == SymbolType.Class;

    public void SetType(SymbolType type)
    {
        _symbolType = type;
    }

    // interface:

    public string Description { get; set; }

    public string? InheritDocs { get; set; }

    string IXmlDocsExtended.Returns
    {
        get => throw new NotSupportedException("no returns for class.");
        set => throw new NotSupportedException("no returns for class.");
    }

    public string Remarks { get; set; }

    public ExamplesDescriptor? Examples { get; set; }

    List<ParamDescriptor> IXmlDocsExtended.Params => throw new NotSupportedException("no params for class.");

    public virtual void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}