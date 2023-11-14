namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Diagnostics;
using RepoM.ActionMenu.Interface.Attributes;

[DebuggerDisplay($"{{{nameof(ContextMenuName)},nq}}")]
public class ActionMenuContextClassDescriptor : ClassDescriptor
{
    public ActionMenuContextAttribute ContextMenuName { get; set; } = null!;

    public override void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}