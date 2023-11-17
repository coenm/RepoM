namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Diagnostics;

[DebuggerDisplay($"{{{nameof(ActionMenuContextObjectName)},nq}}")]
public class ActionMenuContextClassDescriptor : ClassDescriptor
{
    public string ActionMenuContextObjectName => Name;

    public override void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}