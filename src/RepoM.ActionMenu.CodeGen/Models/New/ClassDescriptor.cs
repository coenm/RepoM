namespace RepoM.ActionMenu.CodeGen.Models.New;

using System;
using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ClassDescriptor : IXmlDocsExtended
{
    /// <summary>
    /// Properties, Functions, fields etc. etc.
    /// </summary>
    public List<MemberDescriptor> Members { get; set; } = new List<MemberDescriptor>();

    public string ClassName { get; set; } = null!;
    
    public string Namespace { get; set; } = null!;

    // interface:

    public string Description { get; set; }

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