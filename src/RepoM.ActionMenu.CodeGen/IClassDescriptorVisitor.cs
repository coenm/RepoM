namespace RepoM.ActionMenu.CodeGen;

using RepoM.ActionMenu.CodeGen.Models.New;

public interface IClassDescriptorVisitor
{
    void Visit(ActionMenuContextClassDescriptor descriptor);

    void Visit(ActionMenuClassDescriptor descriptor);

    void Visit(ClassDescriptor descriptor);
}