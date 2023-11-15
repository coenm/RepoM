namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.CodeGen.Models.New;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban;
using Scriban.Runtime;


public interface IClassDescriptorVisitor
{
    void Visit(ActionMenuContextClassDescriptor descriptor);

    void Visit(ActionMenuClassDescriptor descriptor);

    void Visit(ClassDescriptor descriptor);
}


public class ProcessMembersVisitor : IClassDescriptorVisitor
{
    private readonly ITypeSymbol _typeSymbol;

    public ProcessMembersVisitor(ITypeSymbol typeSymbol)
    {
        _typeSymbol = typeSymbol;
    }

    public void Visit(ActionMenuContextClassDescriptor descriptor)
    {
        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            AttributeData? attr = Program.FindAttribute<ActionMenuContextMemberAttribute>(member);
            if (attr == null)
            {
                // normal member.
            }
            else
            {
                var actionMenuContextMemberAttribute = new ActionMenuContextMemberAttribute((attr.ConstructorArguments[0].Value as string)!);
                // action menu context member.

                var className = member.ContainingSymbol.Name;

                //     var method = member as IMethodSymbol;
                //     var desc = new KalkMemberToGenerate()
                //     {
                //         Name = name,
                //         XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                //         Category = string.Empty,
                //         IsCommand = method?.ReturnsVoid ?? false,
                //         Module = moduleToGenerate,
                //     };

                
                var memberDescriptor = new ActionMenuContextMemberDescriptor
                    {
                        ActionMenuContextMemberAttribute = actionMenuContextMemberAttribute,
                        CSharpName = member.Name,
                        //ReturnType = propertyMember.Type.ToDisplayString(), // (member as IPropertySymbol)?.Type;
                        IsCommand = false,
                        XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                    };

                if (member is IMethodSymbol method)
                {
                    memberDescriptor.ReturnType = method.ReturnType.ToDisplayString();
                    memberDescriptor.IsCommand = method.ReturnsVoid;
                }

                if (member is IPropertySymbol property)
                {
                    memberDescriptor.ReturnType = property.Type.ToDisplayString();
                }

            }
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


            //
            // AttributeData? attribute = Program.FindAttribute<RepositoryActionAttribute>(propertyMember);
            //
            // if (attribute == null)
            // {
            //     // normal member.
            //     continue;
            // }

            // if (!typeSymbol.Interfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(actionMenuInterface, SymbolEqualityComparer.Default)))
            // {
            //     continue;
            // }



            descriptor.ActionMenuProperties.Add(memberDescriptor);

            // action menu context member.

 
        }
    }

    public void Visit(ClassDescriptor descriptor)
    {
        foreach (ISymbol member in _typeSymbol.GetMembers())
        {
            // only normal members.
        }
    }
}

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


public class Program
{
    static async Task Main(string[] args)
    {
        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "src");
        var docsFolder = Path.Combine(rootFolder, "docs_new");

        CheckDirectory(srcFolder);
        CheckDirectory(docsFolder);
        CheckDirectory(Path.Combine(rootFolder, ".git"));

        var projects = new List<string>
            {
                "RepoM.ActionMenu.Core",
                
                "RepoM.Plugin.AzureDevOps",
                "RepoM.Plugin.Clipboard",
                "RepoM.Plugin.EverythingFileSearch",
                "RepoM.Plugin.Heidi",
                "RepoM.Plugin.LuceneQueryParser",
                "RepoM.Plugin.SonarCloud",
                "RepoM.Plugin.Statistics",
                "RepoM.Plugin.WebBrowser",
                "RepoM.Plugin.WindowsExplorerGitInfo",
            };

        Template templateModule = await LoadTemplateAsync("Templates/Module.scriban-cs");
        Template templateDocs = await LoadTemplateAsync("Templates/Docs.scriban-txt");

        Dictionary<string, string> files = await LoadFiles();


        var processedProjects = new Dictionary<string, ProjectDescriptor>();

        // project - (class name, module)
        var projectMapping = new Dictionary<string, Dictionary<string, KalkModuleToGenerate>>();
        var projectMappingActions = new Dictionary<string, Dictionary<string, ActionsToGenerate>>();

        foreach (var project in projects)
        {
            var pathToSolution = Path.Combine(srcFolder, project, $"{project}.csproj");
            CheckFile(pathToSolution);
       
            Compilation compilation = await CompilationHelper.CompileAsync(pathToSolution, project);
            
            ProjectDescriptor projectDescriptor;
            AttributeData? assemblyAttribute = compilation.Assembly.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == nameof(PackageAttribute));
            if (assemblyAttribute != null)
            {
                projectDescriptor = new PluginProjectDescriptor
                    {
                        PackageAttribute = new PackageAttribute(
                            (assemblyAttribute.ConstructorArguments[0].Value as string)!,
                            (assemblyAttribute.ConstructorArguments[1].Value as string)!),
                    };
            }
            else
            {
                projectDescriptor = new ProjectDescriptor();
            }

            projectDescriptor.AssemblyName = compilation.AssemblyName ?? throw new Exception("Could not determine AssemblyName");
            projectDescriptor.ProjectName = project;
            
            processedProjects.Add(project, projectDescriptor);

            ProcessProject(compilation, projectDescriptor, files);
            // continue;
            //---
            
            var mapNameToModule = new Dictionary<string, KalkModuleToGenerate>(); // class name, module
            projectMapping.Add(project, mapNameToModule);
            ProcessPossibleActionContextType(files, compilation, mapNameToModule);
            
            var mapNameToActionsModule = new Dictionary<string, ActionsToGenerate>(); // class name, actions
            projectMappingActions.Add(project, mapNameToActionsModule);
            ProcessPossibleActionsType(files, compilation, mapNameToActionsModule);
        }

        foreach ((var project, Dictionary<string, KalkModuleToGenerate>? mapNameToModule) in projectMapping)
        {
            var modules = mapNameToModule.Values.OrderBy(x => x.ClassName).ToList();
            var pathToGeneratedCode = Path.Combine(srcFolder, project, "RepoMCodeGen.generated.cs");

            if (modules.Count == 0)
            {
                // delete if exist:
                if (File.Exists(pathToGeneratedCode))
                {
                    try
                    {
                        File.Delete(pathToGeneratedCode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Could not delete generated file '{pathToGeneratedCode}'.");
                        throw;
                    }
                }

                continue;
            }
            
            var context = new TemplateContext
                {
                    LoopLimit = 0,
                    MemberRenamer = x => x.Name,
                };
            var scriptObject = new ScriptObject()
                {
                    { "modules", modules },
                };
            context.PushGlobal(scriptObject);
        
            var result = await templateModule.RenderAsync(context);
            await File.WriteAllTextAsync(pathToGeneratedCode, result);
        
            // Generate module site documentation
            foreach (KalkModuleToGenerate module in modules)
            {
                await GenerateModuleSiteDocumentation(module, docsFolder, templateDocs);
            }
        }
    }

    private static void ProcessProject(Compilation compilation, ProjectDescriptor projectDescriptor, IDictionary<string, string> files)
    {
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            ProcessMembersVisitor memberVisitor = new(typeSymbol);
            DocsClassVisitor docsClassVisitor = new(typeSymbol, files);

            ClassDescriptor classDescriptor;

            AttributeData? actionMenuContextAttribute = FindAttribute<ActionMenuContextAttribute>(typeSymbol);
            AttributeData? repositoryActionAttribute = FindAttribute<RepositoryActionAttribute>(typeSymbol);
            
            if (actionMenuContextAttribute != null)
            {
                var actionMenuContextClassDescriptor = new ActionMenuContextClassDescriptor
                    {
                        ContextMenuName = new ActionMenuContextAttribute((string) actionMenuContextAttribute.ConstructorArguments[0].Value!),
                    };
                projectDescriptor.ActionContextMenus.Add(actionMenuContextClassDescriptor);

                classDescriptor = actionMenuContextClassDescriptor;
            }
            else if (repositoryActionAttribute != null)
            {
                var actionMenuClassDescriptor = new ActionMenuClassDescriptor
                    {
                        RepositoryActionName = new RepositoryActionAttribute((string)repositoryActionAttribute.ConstructorArguments[0].Value!),
                    };
                projectDescriptor.ActionMenus.Add(actionMenuClassDescriptor);

                classDescriptor = actionMenuClassDescriptor;
            }
            else 
            {
                classDescriptor = new ClassDescriptor();
                projectDescriptor.Types.Add(classDescriptor);
            }

            classDescriptor.ClassName = typeSymbol.Name;
            classDescriptor.Namespace = typeSymbol.ContainingNamespace.ToDisplayString();

            classDescriptor.Accept(docsClassVisitor);
            classDescriptor.Accept(memberVisitor);
            

            // AttributeData? moduleAttribute = FindAttribute<ActionMenuContextAttribute>(typeSymbol);
            // KalkModuleToGenerate? moduleToGenerate = null;
            // if (moduleAttribute != null)
            // {
            //     GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate, mapNameToModule, files);
            // }
            //
            // foreach (ISymbol member in typeSymbol.GetMembers())
            // {
            //     AttributeData? attr = FindAttribute<ActionMenuContextMemberAttribute>(member);
            //     if (attr == null)
            //     {
            //         continue;
            //     }
            //
            //     var name = attr.ConstructorArguments[0].Value?.ToString();
            //     if (string.IsNullOrWhiteSpace(name))
            //     {
            //         throw new Exception("Name cannot be null or empty.");
            //     }
            //
            //     var className = member.ContainingSymbol.Name;
            //
            //     // In case the module is built-in, we still generate a module for it
            //     if (moduleToGenerate == null)
            //     {
            //         GetOrCreateModule(typeSymbol, className, moduleAttribute!, out moduleToGenerate, mapNameToModule, files);
            //     }
            //
            //     var method = member as IMethodSymbol;
            //     var desc = new KalkMemberToGenerate()
            //     {
            //         Name = name,
            //         XmlId = member.GetDocumentationCommentId() ?? string.Empty,
            //         Category = string.Empty,
            //         IsCommand = method?.ReturnsVoid ?? false,
            //         Module = moduleToGenerate,
            //     };
            //     desc.Names.Add(name);
            //
            //     if (method != null)
            //     {
            //         desc.CSharpName = method.Name;
            //
            //         var builder = new StringBuilder();
            //         desc.IsAction = method.ReturnsVoid;
            //         desc.IsFunc = !desc.IsAction;
            //         builder.Append(desc.IsAction ? "Action" : "Func");
            //
            //         if (method.Parameters.Length > 0 || desc.IsFunc)
            //         {
            //             builder.Append('<');
            //         }
            //
            //         for (var i = 0; i < method.Parameters.Length; i++)
            //         {
            //             IParameterSymbol parameter = method.Parameters[i];
            //             if (i > 0)
            //             {
            //                 builder.Append(", ");
            //             }
            //
            //             builder.Append(GetTypeName(parameter.Type));
            //         }
            //
            //         if (desc.IsFunc)
            //         {
            //             if (method.Parameters.Length > 0)
            //             {
            //                 builder.Append(", ");
            //             }
            //             builder.Append(GetTypeName(method.ReturnType));
            //         }
            //
            //         if (method.Parameters.Length > 0 || desc.IsFunc)
            //         {
            //             builder.Append('>');
            //         }
            //
            //         desc.Cast = $"({builder})";
            //     }
            //     else if (member is IPropertySymbol or IFieldSymbol)
            //     {
            //         desc.CSharpName = member.Name;
            //         desc.IsConst = true;
            //     }
            //
            //     moduleToGenerate.Members.Add(desc);
            //     XmlDocsParser.ExtractDocumentation(member, desc, files);
            // }
        }
    }

    private static void ProcessPossibleActionsType(Dictionary<string, string> files, Compilation compilation, Dictionary<string, ActionsToGenerate> mapNameToActionsModule)
    {
        INamedTypeSymbol? actionMenuInterface = TypeMatcher.GetTypeSymbolForType(typeof(IMenuAction), compilation);
        if (actionMenuInterface == null)
        {
            Console.WriteLine($"Could not create/find NamedSymbol for {nameof(IMenuAction)}.");
            return;
        }
        
        var typeToNamedTypeMapping = new Dictionary<Type, INamedTypeSymbol?>
            {
                { typeof(IMenuAction), actionMenuInterface },
                // { typeof(IEnabled), TypeMatcher.GetTypeSymbolForType(typeof(IEnabled), compilation) },
                // { typeof(IName), TypeMatcher.GetTypeSymbolForType(typeof(IName), compilation) },
            };
        
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            if (!typeSymbol.Interfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(actionMenuInterface, SymbolEqualityComparer.Default)))
            {
                continue;
            }

            var attribute = FindAttribute<RepositoryActionAttribute>(typeSymbol);
            if (attribute == null)
            {
                throw new Exception("A type should have a RepositoryActionAttribute.");
            }

            if (attribute.ConstructorArguments.Length != 1)
            {
                throw new Exception($"Unexpected number of arguments for {nameof(RepositoryActionAttribute)}");
            }

            string? s = attribute.ConstructorArguments[0].Value as string;
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new Exception("Argument is not a valid string.");
            }

            var repositoryActionAttribute = new RepositoryActionAttribute(s);
            
            // at this point, we know the type is an action.
            // create object for documenation and process all relevant properties
            ProcessRepositoryActionType(typeSymbol, repositoryActionAttribute, mapNameToActionsModule, files, typeToNamedTypeMapping);
        }
    }
    
    private static void ProcessPossibleActionContextType(Dictionary<string, string> files, Compilation compilation, Dictionary<string, KalkModuleToGenerate> mapNameToModule)
    {
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            AttributeData? moduleAttribute = FindAttribute<ActionMenuContextAttribute>(typeSymbol);
            KalkModuleToGenerate? moduleToGenerate = null;
            if (moduleAttribute != null)
            {
                GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate, mapNameToModule, files);
            }

            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                AttributeData? attr = FindAttribute<ActionMenuContextMemberAttribute>(member);
                if (attr == null)
                {
                    continue;
                }

                var name = attr.ConstructorArguments[0].Value?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Name cannot be null or empty.");
                }

                var className = member.ContainingSymbol.Name;

                // In case the module is built-in, we still generate a module for it
                if (moduleToGenerate == null)
                {
                    GetOrCreateModule(typeSymbol, className, moduleAttribute!, out moduleToGenerate, mapNameToModule, files);
                }

                var method = member as IMethodSymbol;
                var desc = new KalkMemberToGenerate()
                {
                    Name = name,
                    XmlId = member.GetDocumentationCommentId() ?? string.Empty,
                    Category = string.Empty,
                    IsCommand = method?.ReturnsVoid ?? false,
                    Module = moduleToGenerate,
                };
                desc.Names.Add(name);

                if (method != null)
                {
                    desc.CSharpName = method.Name;

                    var builder = new StringBuilder();
                    desc.IsAction = method.ReturnsVoid;
                    desc.IsFunc = !desc.IsAction;
                    builder.Append(desc.IsAction ? "Action" : "Func");

                    if (method.Parameters.Length > 0 || desc.IsFunc)
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

                        builder.Append(GetTypeName(parameter.Type));
                    }

                    if (desc.IsFunc)
                    {
                        if (method.Parameters.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(GetTypeName(method.ReturnType));
                    }

                    if (method.Parameters.Length > 0 || desc.IsFunc)
                    {
                        builder.Append('>');
                    }

                    desc.Cast = $"({builder})";
                }
                else if (member is IPropertySymbol or IFieldSymbol)
                {
                    desc.CSharpName = member.Name;
                    desc.IsConst = true;
                }

                moduleToGenerate.Members.Add(desc);
                XmlDocsParser.ExtractDocumentation(member, desc, files);
            }
        }
    }

    private static void ProcessRepositoryActionType(
        ITypeSymbol typeSymbol,
        RepositoryActionAttribute repositoryActionAttribute,
        Dictionary<string, ActionsToGenerate> mapNameToActionsModule,
        Dictionary<string, string> files,
        Dictionary<Type, INamedTypeSymbol?> typeToNamedTypeMapping)
    {
        Console.WriteLine(typeSymbol.Name);
        ActionsToGenerate? moduleToGenerate;
        GetOrCreateActionModule(
            typeSymbol,
            typeSymbol.Name,
            repositoryActionAttribute,
            out moduleToGenerate,
            mapNameToActionsModule,
            files);

        if (moduleToGenerate == null)
        {
            throw new Exception("Could not create Actions modules");
        }

        List<ISymbol> interestingMembers = new();

        Console.WriteLine($"{repositoryActionAttribute.Type}");

        foreach (ISymbol member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertyMember)
            {
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

            // todo skip Type property?!


            Console.WriteLine($"-  {propertyMember.Name}");
            interestingMembers.Add(propertyMember);

            foreach (var att in propertyMember.GetAttributes())
            {
                Console.WriteLine($"  -  {att.AttributeClass.Name}");
            }

            continue;

            AttributeData? attr = FindAttribute<ActionMenuContextMemberAttribute>(propertyMember);
            if (attr == null)
            {
                continue;
            }
            
            var name = attr.ConstructorArguments[0].Value?.ToString();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Name cannot be null or empty.");
            }

            var className = propertyMember.ContainingSymbol.Name;

            // // In case the module is built-in, we still generate a module for it
            // if (moduleToGenerate == null)
            // {
            //     GetOrCreateModule(typeSymbol, className, moduleAttribute!, out moduleToGenerate, mapNameToModule, files);
            // }

            var propertyDescription = new ActionPropertyToGenerate()
                {
                    Name = name,
                    XmlId = propertyMember.GetDocumentationCommentId() ?? string.Empty,
                    Category = string.Empty, // todo? 
                    IsCommand = false, // todo remove?
                    IsBuiltin = !member.ContainingNamespace.Name.Contains("Plugin"), //todo
                    // Module = moduleToGenerate,
                };
            propertyDescription.Names.Add(name);

            propertyDescription.CSharpName = propertyMember.Name;
            propertyDescription.IsConst = true;

            moduleToGenerate.Properties.Add(propertyDescription);
            XmlDocsParser.ExtractDocumentation(propertyMember, propertyDescription, files);
        }

        // first the name
        // var m = interestingMembers.Find(item => item.Name == nameof(IName.Name));
        // if (m != null)
        // {
        //     interestingMembers.Remove(m);
        //     Cr(m);
        // }

    }
    
    private static void GetOrCreateActionModule(
        ITypeSymbol typeSymbol,
        string className,
        RepositoryActionAttribute moduleAttribute,
        out ActionsToGenerate moduleToGenerate,
        IDictionary<string, ActionsToGenerate> mapNameToModule,
        IDictionary<string, string> files)
    {
        var ns = typeSymbol.ContainingNamespace.ToDisplayString();

        var fullClassName = $"{ns}.{className}";
        if (mapNameToModule.TryGetValue(fullClassName, out moduleToGenerate))
        {
            return;
        }

        moduleToGenerate = new ActionsToGenerate()
            {
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                ClassName = className,
            };
        mapNameToModule.Add(fullClassName, moduleToGenerate);

        // if (moduleAttribute != null)
        // {
            moduleToGenerate.Name = moduleAttribute.Type;
            moduleToGenerate.Names.Add(moduleToGenerate.Name!);
            moduleToGenerate.Category = "Modules (e.g `import Files`)";
        // }
        // else
        // {
        //     moduleToGenerate.Name = className.Replace("Module", "");
        //     moduleToGenerate.IsBuiltin = true;
        // }

        XmlDocsParser.ExtractDocumentation(typeSymbol, moduleToGenerate, files);
    }
    
    public static AttributeData? FindAttribute<T>(ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass!.Name == typeof(T).Name);
    }

    private static void GetOrCreateModule(
        ITypeSymbol typeSymbol,
        string className,
        AttributeData moduleAttribute,
        out KalkModuleToGenerate moduleToGenerate,
        IDictionary<string, KalkModuleToGenerate> mapNameToModule,
        IDictionary<string, string> files)
    {
        var ns = typeSymbol.ContainingNamespace.ToDisplayString();

        var fullClassName = $"{ns}.{className}";
        if (mapNameToModule.TryGetValue(fullClassName, out moduleToGenerate))
        {
            return;
        }

        moduleToGenerate = new KalkModuleToGenerate()
            {
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                ClassName = className,
            };
        mapNameToModule.Add(fullClassName, moduleToGenerate);

        if (moduleAttribute != null)
        {
            moduleToGenerate.Name = moduleAttribute.ConstructorArguments[0].Value.ToString();
            moduleToGenerate.Names.Add(moduleToGenerate.Name!);
            moduleToGenerate.Category = "Modules (e.g `import Files`)";
        }
        else
        {
            moduleToGenerate.Name = className.Replace("Module", "");
            moduleToGenerate.IsBuiltin = true;
        }

        XmlDocsParser.ExtractDocumentation(typeSymbol, moduleToGenerate, files);
    }
    
    private static async Task GenerateModuleSiteDocumentation(KalkModuleToGenerate module, string siteFolder, Template template)
    {
        var result = await DocumentationGenerator.GetDocsContentAsync(module, template);
        var name = module.Name.ToLowerInvariant();
        await File.WriteAllTextAsync(Path.Combine(siteFolder, $"{name}.generated.md"), result);
    }
    
    static string GetTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString();
    }
    
    private static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new Exception($"Folder '{path}' does not exist");
        }
    }

    private static void CheckFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"File '{path}' does not exist");
        }
    }

    private static async Task<Template> LoadTemplateAsync(string path)
    {
        var rawTemplate = await File.ReadAllTextAsync(path);
        var template = Template.Parse(rawTemplate);
        if (template.HasErrors)
        {
            throw new Exception(template.Messages.ToString());
        }

        return template;
    }

    private static async Task<Dictionary<string, string>> LoadFiles()
    {
        string[] files = Directory.GetFiles("C:\\Projects\\Private\\git\\RepoM\\docs\\snippets"); // todo

        var result = new Dictionary<string, string>(files.Length);

        foreach (string file in files)
        {
            var f = new FileInfo(file);
            var fileContent = await File.ReadAllTextAsync(file);
            result.Add(f.Name, fileContent);
        }

        return result;
    }
}