namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Misc;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin;
using Text = Interface.YamlModel.Templating.Text;

public class ProcessMembersVisitor : IClassDescriptorVisitor
{
    private readonly ITypeSymbol _typeSymbol;
    private readonly IDictionary<string, string> _files;

    // todo extend.
    private static readonly string[] _collectionTypes =
    [
        "System.Collections.Generic.List<T>",
        "System.Collections.Generic.IList<T>",
        "System.Collections.Generic.IEnumerable<T>",
    ];

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
                memberDescriptor.ReturnType = TypeInfoDescriptorFactory.Create(method.ReturnType);
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
                memberDescriptor.ReturnType = TypeInfoDescriptorFactory.Create(property.Type);
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

            // Name = name,
            // XmlId = member.GetDocumentationCommentId() ?? string.Empty,
            // Category = string.Empty,
            // IsCommand = method?.ReturnsVoid ?? false,
            // Module = moduleToGenerate,

            SymbolDisplayFormat symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining).WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.None);

            void JustSomeCode(ITypeSymbol symbol)
            {
                var Name = symbol.ToDisplayString(symbolDisplayFormat);
                var IsEnumerable = symbol.AllInterfaces.Any(x => x.ToString() == "System.Collections.IEnumerable");
                var IsVoid = symbol.SpecialType == SpecialType.System_Void;
                if (IsEnumerable && (symbol is INamedTypeSymbol namedSymbol))
                {
                    var firstTypeArgumentName = namedSymbol.TypeArguments.FirstOrDefault()?.ToDisplayString(symbolDisplayFormat);
                }
            }

            static bool IsSystemType(IPropertySymbol symbol)
            {
                var typeFullName = symbol.Type.ToString();
                return typeFullName?.StartsWith("System.") ?? false;
            }

            // single type in collection (array, list, ..) (no tuple or whatsoever)
            static bool IsCollection(IPropertySymbol symbol, [NotNullWhen(true)] out ITypeSymbol? genericType)
            {
                if (symbol.Type is IArrayTypeSymbol ats)
                {
                    genericType = ats.ElementType;
                    return true;
                }

                var originalDefinitionDisplayName = symbol.Type.OriginalDefinition.ToDisplayString();

                var displayString = symbol.Type.ToDisplayString();

                if (_collectionTypes.Contains(originalDefinitionDisplayName))
                {
                    // must be singe due to <T>
                    genericType = ((INamedTypeSymbol)symbol.Type).TypeArguments.Single();
                    return true;
                }
                
                genericType = null;
                return false;
            }

            var propertyReturnType = propertyMember.Type.ToDisplayString();

            bool IsTypeOrNullableType<T>()
            {
                var typeFullName = typeof(T).FullName ?? string.Empty;
                return propertyReturnType.Equals(typeFullName) || propertyReturnType.Equals(typeFullName + "?");
            }
            
            var memberDescriptor = new ActionMenuMemberDescriptor
                {
                    CSharpName = propertyMember.Name,
                    ReturnType = TypeInfoDescriptorFactory.Create(propertyMember.Type), // (member as IPropertySymbol)?.Type;
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                };

            if (IsTypeOrNullableType<Text>())
            {
                memberDescriptor.IsTemplate = true;

                AttributeData? attr = propertyMember.FindAttribute<TextAttribute>();
                if (attr?.ConstructorArguments.Length == 1)
                {
                    var textAttribute = new TextAttribute((attr.ConstructorArguments[0].Value as string)!);
                    memberDescriptor.DefaultValue = textAttribute.DefaultValue;
                }
            }
            else if (IsTypeOrNullableType<Predicate>())
            {
                memberDescriptor.IsPredicate = true;

                AttributeData? attr = propertyMember.FindAttribute<PredicateAttribute>();
                if (attr != null)
                {
                    var predicateAttribute = new PredicateAttribute((bool)attr.ConstructorArguments[0].Value!);
                    memberDescriptor.DefaultValue = predicateAttribute.DefaultValue;
                }
            }
            else if (IsTypeOrNullableType<Context>())
            {
                memberDescriptor.IsContext = true;
            }
            else if (IsCollection(propertyMember, out ITypeSymbol? genericType))
            {
                // todo
                // is is a collection of x. process x
                memberDescriptor.IsReturnEnumerable = true;
            }
            else if (IsSystemType(propertyMember))
            {
                // ie string, int, bool, ..
                Console.WriteLine("d");
            }
            else if (propertyReturnType.Contains("RepoM.ActionMenu.CodeGenDummyLibrary.ActionMenu.Model.ActionMenus.AutoCompleteOptionsV1"))
            {
                // aditional checks?
                // todo, name
                memberDescriptor.RefType = $"{propertyMember.ContainingModule.Name}; {propertyReturnType}";
            }

            // if (!typeSymbol.Interfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(actionMenuInterface, SymbolEqualityComparer.Default)))
            // {
            //     continue;
            // }
            
            XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);

            if (string.IsNullOrWhiteSpace(memberDescriptor.Description) && string.IsNullOrWhiteSpace(memberDescriptor.InheritDocs))
            {
                if (!memberDescriptor.CSharpName.Equals("Type"))
                {
                    Console.WriteLine($"Skip property '{_typeSymbol.Name}.{memberDescriptor.CSharpName}' due to missing description");
                }
                
                continue;
            }

            descriptor.ActionMenuProperties.Add(memberDescriptor);
        }
    }

    public void Visit(ModuleConfigurationClassDescriptor descriptor)
    {
        Visit(descriptor as ClassDescriptor);

        AttributeData? moduleConfigurationAttribute = _typeSymbol.FindAttribute<ModuleConfigurationAttribute>();

        if (moduleConfigurationAttribute == null)
        {
            return;
        }

        var version = (int)moduleConfigurationAttribute.ConstructorArguments[0].Value!;

        ISymbol? defaultFactoryMethodSymbol = _typeSymbol.GetMembers().SingleOrDefault(symbol => symbol.FindAttribute<ModuleConfigurationDefaultValueFactoryMethodAttribute>() != null);
        if (!(defaultFactoryMethodSymbol is IMethodSymbol methodSymbol))
        {
            return;
        }

        if (descriptor.DotNetType == null)
        {
            throw new Exception(descriptor.FullName + "  No type found for ModuleConfigurationDefaultValueFactoryMethodAttribute");
        }

        var defaultValueResult = descriptor.DotNetType.InvokeMember(
            methodSymbol.Name,
            BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
            null,
            null,
            null);

        descriptor.DefaultValueJson = FileBasedPackageConfiguration.SerializeConfiguration(defaultValueResult, version);

        // 
        ISymbol? exampleFactoryMethodSymbol = _typeSymbol.GetMembers().SingleOrDefault(symbol => symbol.FindAttribute<ModuleConfigurationExampleValueFactoryMethodAttribute>() != null);
        if (!(exampleFactoryMethodSymbol is IMethodSymbol methodSymbolExample))
        {
            return;
        }

        var exampleValueResult = descriptor.DotNetType.InvokeMember(
            methodSymbolExample.Name,
            BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
            null,
            null,
            null);

        descriptor.ExampleValueJson = FileBasedPackageConfiguration.SerializeConfiguration(exampleValueResult, version);
    }

    public void Visit(ClassDescriptor descriptor)
    {
        if (_typeSymbol is INamedTypeSymbol { TypeKind: TypeKind.Enum, } symbol)
        {
            descriptor.SetType(SymbolType.Enum);

            // INamedTypeSymbol? underlyingType = symbol.EnumUnderlyingType;

            var memberNames = _typeSymbol
                              .GetMembers()
                              .Where(static member => member.Kind is SymbolKind.Field)
                              // .Select(static symbol => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                              ;

            foreach (ISymbol member in memberNames)
            {
                var memberDescriptor = new MemberDescriptor
                    {
                        CSharpName = member.Name,
                        IsCommand = false,
                        XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                    };

                XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);
                descriptor.Members.Add(memberDescriptor);
            }

            return;
        }

        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertyMember)
            {
                // only interested in properties.
                continue;
            }

            if (!member.CanBeReferencedByName)
            {
                continue;
            }

            if (member.DeclaredAccessibility == Accessibility.Private)
            {
                continue;
            }

            if (member.IsStatic)
            {
                continue;
            }


            // only normal members.
            var className = member.ContainingSymbol.Name;

            var memberDescriptor = new MemberDescriptor
                {
                    CSharpName = member.Name,
                    //ReturnType = propertyMember.Type.ToDisplayString(), // (member as IPropertySymbol)?.Type;
                    IsCommand = false,
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                };

            if (member is IMethodSymbol method)
            {
                memberDescriptor.ReturnType = TypeInfoDescriptorFactory.Create(method.ReturnType);
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
                memberDescriptor.ReturnType = TypeInfoDescriptorFactory.Create(property.Type);
                memberDescriptor.IsConst = true;
            }

            descriptor.Members.Add(memberDescriptor);

            XmlDocsParser.ExtractDocumentation(member, memberDescriptor, _files);
        }
    }
}