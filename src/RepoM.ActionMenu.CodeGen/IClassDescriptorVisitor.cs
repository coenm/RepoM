namespace RepoM.ActionMenu.CodeGen;

using RepoM.ActionMenu.CodeGen.Models;

public interface IClassDescriptorVisitor
{
    void Visit(ActionMenuContextClassDescriptor descriptor);

    void Visit(ActionMenuClassDescriptor descriptor);

    void Visit(ClassDescriptor descriptor);
}