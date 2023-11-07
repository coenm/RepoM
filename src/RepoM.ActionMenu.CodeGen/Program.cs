namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;
using Scriban;
using Scriban.Runtime;

public class Program
{
    static bool TypeSymbolMatchesType(ITypeSymbol typeSymbol, Type type, Compilation compilation)
    {
        return GetTypeSymbolForType(type, compilation).Equals(typeSymbol);
    }

    static INamedTypeSymbol GetTypeSymbolForType(Type type, Compilation compilation)
    {
        if (!type.IsConstructedGenericType)
        {
            return compilation.GetTypeByMetadataName(type.FullName!);
        }

        // get all typeInfo's for the Type arguments 
        var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, compilation));

        var openType = type.GetGenericTypeDefinition();
        var typeSymbol = compilation.GetTypeByMetadataName(openType.FullName!);
        return typeSymbol.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
    }

    static async Task Main(string[] args)
    {
        // not sure why Kalk has this.
        _ = typeof(System.Composition.CompositionContext).Name;

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
                // "RepoM.Plugin.EverythingFileSearch",
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

        var projectMapping = new Dictionary<string, Dictionary<string, KalkModuleToGenerate>>();

        foreach (var project in projects)
        {
            var pathToSolution = Path.Combine(srcFolder, project, $"{project}.csproj");
           
            CheckFile(pathToSolution);
       
            Compilation compilation = await CompilationHelper.CompileAsync(pathToSolution, project);

            INamedTypeSymbol? actionMenuInterface = GetTypeSymbolForType(typeof(IMenuAction), compilation);
            if (actionMenuInterface == null)
            {
                throw new Exception($"Could not create/find NamedSymbol for {nameof(IMenuAction)}.");
            }

            var mapNameToModule = new Dictionary<string, KalkModuleToGenerate>();
            projectMapping.Add(project, mapNameToModule);

            foreach (ISymbol type in compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type))
            {
                if (type is not ITypeSymbol typeSymbol)
                {
                    continue;
                }

                AttributeData? moduleAttribute = FindAttribute<ActionMenuContextAttribute>(typeSymbol);
                KalkModuleToGenerate? moduleToGenerate = null;
                if (moduleAttribute != null)
                {
                    GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate, mapNameToModule, files);
                }

                if (typeSymbol.Interfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(actionMenuInterface, SymbolEqualityComparer.Default)))
                {
                    // found, works
                    Console.WriteLine(typeSymbol.Name);
                    continue;
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

        foreach ((var project, Dictionary<string, KalkModuleToGenerate>? mapNameToModule) in projectMapping)
        {
            var modules = mapNameToModule.Values.OrderBy(x => x.ClassName).ToList();
            var pathToGeneratedCode = Path.Combine(srcFolder, project, "RepoMCodeGen.generated.cs");
        
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

    internal static AttributeData? FindAttribute<T>(ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass!.Name == typeof(T).Name);
    }

    private static void GetOrCreateModule(
        ITypeSymbol typeSymbol,
        string className,
        AttributeData moduleAttribute,
        out KalkModuleToGenerate moduleToGenerate,
        IDictionary<string, KalkModuleToGenerate>? mapNameToModule,
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