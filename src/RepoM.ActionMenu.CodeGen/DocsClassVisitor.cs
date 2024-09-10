namespace RepoM.ActionMenu.CodeGen;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;

public class DocsClassVisitor : IClassDescriptorVisitor
{
    private readonly ITypeSymbol _typeSymbol;
    private readonly IDictionary<string, string> _files;

    public DocsClassVisitor(ITypeSymbol typeSymbol, IDictionary<string, string> files)
    {
        _typeSymbol = typeSymbol;
        _files = files;
    }

    public void Visit(ActionMenuContextClassDescriptor descriptor)
    {
        Visit(descriptor as ClassDescriptor);
    }

    public void Visit(ActionMenuClassDescriptor descriptor)
    {
        Visit(descriptor as ClassDescriptor);
    }

    public void Visit(ModuleConfigurationClassDescriptor descriptor)
    {
        Visit(descriptor as ClassDescriptor);
    }

    public void Visit(ClassDescriptor descriptor)
    {
        Misc.XmlDocsParser.ExtractDocumentation(_typeSymbol, descriptor, _files);
    }
}