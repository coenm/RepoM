namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Diagnostics;
using RepoM.ActionMenu.Interface.Attributes;

[DebuggerDisplay($"{{{nameof(ContextMenuName)},nq}}")]
public class ActionMenuContextClassDescriptor : ClassDescriptor
{
    public ActionMenuContextAttribute ContextMenuName { get; set; }

    public override void Accept(IClassDescriptorVisitor classDescriptorVisitor)
    {
        classDescriptorVisitor.Visit(this);
    }
}