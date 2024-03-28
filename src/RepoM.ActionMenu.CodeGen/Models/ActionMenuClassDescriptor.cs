namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ActionMenuClassDescriptor : ClassDescriptor
{
    /// <summary>
    /// Properties
    /// </summary>
    public List<ActionMenuMemberDescriptor> ActionMenuProperties { get; set; } = new List<ActionMenuMemberDescriptor>();

    public string RepositoryActionName => Name;

    public override void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}