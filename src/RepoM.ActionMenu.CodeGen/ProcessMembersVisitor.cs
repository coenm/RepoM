namespace RepoM.ActionMenu.CodeGen;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models.New;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

public class ProcessMembersVisitor : IClassDescriptorVisitor
{
    private readonly ITypeSymbol _typeSymbol;
    private readonly IDictionary<string, string> _files;

    public ProcessMembersVisitor(ITypeSymbol typeSymbol, IDictionary<string, string> files)
    {
        _typeSymbol = typeSymbol;
        _files = files;
    }

    public void Visit(ActionMenuContextClassDescriptor descriptor)
    {
        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            AttributeData? attr = member.FindAttribute<ActionMenuContextMemberAttribute>();
            if (attr == null)
            {
                // normal member -> skip and continue.
                continue;
            }

            var actionMenuContextMemberAttribute = new ActionMenuContextMemberAttribute((attr.ConstructorArguments[0].Value as string)!);
            // action menu context member.

            var className = member.ContainingSymbol.Name;
                
            var memberDescriptor = new ActionMenuContextMemberDescriptor
                {
                    Name = actionMenuContextMemberAttribute.Alias,
                    CSharpName = member.Name,
                    //ReturnType = propertyMember.Type.ToDisplayString(), // (member as IPropertySymbol)?.Type;
                    IsCommand = false,
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                };

            if (member is IMethodSymbol method)
            {
                memberDescriptor.ReturnType = method.ReturnType.ToDisplayString();
                memberDescriptor.IsCommand = method.ReturnsVoid;

                memberDescriptor.CSharpName = method.Name;

                memberDescriptor.IsAction = method.ReturnsVoid;
                memberDescriptor.IsFunc = !memberDescriptor.IsAction;

                var builder = new StringBuilder();
                builder.Append(memberDescriptor.IsAction ? "Action" : "Func");

                if (method.Parameters.Length > 0 || memberDescriptor.IsFunc)
                {
                    builder.Append('<');
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    IParameterSymbol parameter = method.Parameters[i];
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(parameter.Type.ToDisplayString());
                }

                if (memberDescriptor.IsFunc)
                {
                    if (method.Parameters.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(method.ReturnType.ToDisplayString());
                }

                if (method.Parameters.Length > 0 || memberDescriptor.IsFunc)
                {
                    builder.Append('>');
                }

                memberDescriptor.Cast = $"({builder})";
            }

            if (member is IPropertySymbol property) // or field IFieldSymbol
            {
                memberDescriptor.ReturnType = property.Type.ToDisplayString();
                memberDescriptor.IsConst = true;
            }

            descriptor.Members.Add(memberDescriptor);

            XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);
        }
    }

    public void Visit(ActionMenuClassDescriptor descriptor)
    {
        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertyMember)
            {
                continue;
            }

            if (propertyMember.SetMethod == null)
            {
                // property is readonly
                continue;
            }
            if (propertyMember.GetMethod == null)
            {
                // property is writeonly
                continue;
            }

            // coen

            // Name = name,
            // XmlId = member.GetDocumentationCommentId() ?? string.Empty,
            // Category = string.Empty,
            // IsCommand = method?.ReturnsVoid ?? false,
            // Module = moduleToGenerate,

            SymbolDisplayFormat symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining).WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.None);

            void Coen(ITypeSymbol symbol)
            {
                var Name = symbol.ToDisplayString(symbolDisplayFormat);
                var IsEnumerable = symbol.AllInterfaces.Any(x => x.ToString() == "System.Collections.IEnumerable");
                var IsVoid = symbol.SpecialType == SpecialType.System_Void;
                if (IsEnumerable && (symbol is INamedTypeSymbol namedSymbol))
                {
                    var FirstTypeArgumentName = namedSymbol.TypeArguments.FirstOrDefault()?.ToDisplayString(symbolDisplayFormat);
                }
            }
            
            var memberDescriptor = new ActionMenuMemberDescriptor
                {
                    CSharpName = propertyMember.Name,
                    ReturnType = propertyMember.Type.ToDisplayString(), // (member as IPropertySymbol)?.Type;
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                };

            if (propertyMember.Type.ToDisplayString().Equals(typeof(Text).FullName))
            {
                memberDescriptor.IsTemplate = true;

                AttributeData? attr = propertyMember.FindAttribute<TextAttribute>();
                if (attr?.ConstructorArguments.Length == 1)
                {
                    var textAttribute = new TextAttribute((attr.ConstructorArguments[0].Value as string)!);
                    memberDescriptor.DefaultValue = textAttribute.DefaultValue;
                }
            }
            else if (propertyMember.Type.ToDisplayString().Equals(typeof(Predicate).FullName))
            {
                memberDescriptor.IsPredicate = true;

                AttributeData? attr = propertyMember.FindAttribute<PredicateAttribute>();
                if (attr != null)
                {
                    var predicateAttribute = new PredicateAttribute((bool)attr.ConstructorArguments[0].Value!);
                    memberDescriptor.DefaultValue = predicateAttribute.DefaultValue;
                }
            }

            // if (!typeSymbol.Interfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(actionMenuInterface, SymbolEqualityComparer.Default)))
            // {
            //     continue;
            // }

            // todo lots of possible attributes to process


            descriptor.ActionMenuProperties.Add(memberDescriptor);

            XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);
        }
    }

    public void Visit(ClassDescriptor descriptor)
    {
        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            // only normal members.
            var className = member.ContainingSymbol.Name;

            var memberDescriptor = new Models.New.MemberDescriptor
                {
                    CSharpName = member.Name,
                    //ReturnType = propertyMember.Type.ToDisplayString(), // (member as IPropertySymbol)?.Type;
                    IsCommand = false,
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                };

            if (member is IMethodSymbol method)
            {
                memberDescriptor.ReturnType = method.ReturnType.ToDisplayString();
                memberDescriptor.IsCommand = method.ReturnsVoid;

                memberDescriptor.CSharpName = method.Name;

                memberDescriptor.IsAction = method.ReturnsVoid;
                memberDescriptor.IsFunc = !memberDescriptor.IsAction;

                var builder = new StringBuilder();
                builder.Append(memberDescriptor.IsAction ? "Action" : "Func");

                if (method.Parameters.Length > 0 || memberDescriptor.IsFunc)
                {
                    builder.Append('<');
                }

                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    IParameterSymbol parameter = method.Parameters[i];
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(parameter.Type.ToDisplayString());
                }

                if (memberDescriptor.IsFunc)
                {
                    if (method.Parameters.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(method.ReturnType.ToDisplayString());
                }

                if (method.Parameters.Length > 0 || memberDescriptor.IsFunc)
                {
                    builder.Append('>');
                }

                memberDescriptor.Cast = $"({builder})";
            }

            if (member is IPropertySymbol property) // or field IFieldSymbol
            {
                memberDescriptor.ReturnType = property.Type.ToDisplayString();
                memberDescriptor.IsConst = true;
            }

            XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);
        }
    }
}