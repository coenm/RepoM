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
        XmlDocsParser.ExtractDocumentation(_typeSymbol, descriptor, _files);
    }

    public void Visit(ActionMenuClassDescriptor descriptor)
    {
        XmlDocsParser.ExtractDocumentation(_typeSymbol, descriptor, _files);
    }

    public void Visit(ClassDescriptor descriptor)
    {
        XmlDocsParser.ExtractDocumentation(_typeSymbol, descriptor, _files);
    }
}