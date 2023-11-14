namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Diagnostics;

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ActionMenuClassDescriptor : ClassDescriptor
{
    public override void Accept(IClassDescriptorVisitor classDescriptorVisitor)
    {
        classDescriptorVisitor.Visit(this);
    }
}