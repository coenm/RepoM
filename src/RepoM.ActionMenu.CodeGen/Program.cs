namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.CodeGen.Models.New;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban;
using Scriban.Runtime;

public static class Program
{
    public static async Task Main()
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

        Template templateModule = await LoadTemplateAsync("Templates/ScribanModuleRegistration.scriban-cs");
        Template templateDocs = await LoadTemplateAsync("Templates/DocsScriptVariables.scriban-txt");

        Dictionary<string, string> files = await LoadFiles();
        
        var processedProjects = new Dictionary<string, ProjectDescriptor>(); // project name -> project descriptor object

        // project - (class name, module)
        var projectMapping = new Dictionary<string, Dictionary<string, KalkModuleToGenerate>>();
        
        foreach (var project in projects)
        {
            var pathToSolution = Path.Combine(srcFolder, project, $"{project}.csproj");
            CheckFile(pathToSolution);

            (Compilation compilation, ProjectDescriptor projectDescriptor) = await CompileAndExtractProjectDescription(pathToSolution, project, files);
            
            processedProjects.Add(project, projectDescriptor);

            // continue;
            //---
            var mapNameToModule = new Dictionary<string, KalkModuleToGenerate>(); // class name, module
            projectMapping.Add(project, mapNameToModule);
            ProcessPossibleActionContextType(files, compilation, mapNameToModule);
        }

        // new style
        // Generate module site documentation
        foreach ((var projectName, ProjectDescriptor? project) in processedProjects)
        {
            foreach (ActionMenuContextClassDescriptor actionContextMenu in project.ActionContextMenus)
            {
                (string fileName, string content) = await GenerateModuleSiteDocumentationFromProjectDescription(actionContextMenu, templateDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(docsFolder, fileName), content).ConfigureAwait(false);
            }
        }
        // end of new style
        
        // tmp
        var json = JsonConvert.SerializeObject(processedProjects, Formatting.Indented);
        await File.WriteAllTextAsync("C:\\tmp\\repom.export.json", json);

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

            if (!processedProjects.TryGetValue(project, out ProjectDescriptor? projectNew))
            {
                throw new Exception("No project found");
            }
            var modules2 = projectNew.ActionContextMenus.OrderBy(x => x.ClassName).ToList();

            var context = new TemplateContext
                {
                    LoopLimit = 0,
                    MemberRenamer = x => x.Name,
                    EnableRelaxedMemberAccess = false,
                };

            var scriptObject = new ScriptObject()
                {
                    { "modules", modules2 },
                };
            context.PushGlobal(scriptObject);
        
            var result = await templateModule.RenderAsync(context);
            await File.WriteAllTextAsync(pathToGeneratedCode, result);
        
            // foreach (var module in modules2)
            // {
            //     await GenerateModuleSiteDocumentation(module, docsFolder, templateDocs);
            // }
        }
    }

    public static async Task<(Compilation compilation, ProjectDescriptor projectDescriptor)> CompileAndExtractProjectDescription(string pathToSolution, string project, IDictionary<string, string> files)
    {
        Compilation compilation = await CompilationHelper.CompileAsync(pathToSolution, project).ConfigureAwait(false);

        var projectDescriptor = new ProjectDescriptor
            {
                AssemblyName = compilation.AssemblyName ?? throw new Exception("Could not determine AssemblyName"),
                ProjectName = project,
            };

        AttributeData? assemblyAttribute = compilation.Assembly.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == nameof(PackageAttribute));
        if (assemblyAttribute != null)
        {
            var pa = new PackageAttribute(
                (assemblyAttribute.ConstructorArguments[0].Value as string)!,
                (assemblyAttribute.ConstructorArguments[1].Value as string)!);
            projectDescriptor.SetPackageInformation(pa);
        }

        ProcessProject(compilation, projectDescriptor, files);

        return (compilation, projectDescriptor);
    }

    private static void ProcessProject(Compilation compilation, ProjectDescriptor projectDescriptor, IDictionary<string, string> files)
    {
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            ProcessMembersVisitor memberVisitor = new(typeSymbol, files);
            DocsClassVisitor docsClassVisitor = new(typeSymbol, files);

            ClassDescriptor classDescriptor;

            AttributeData? actionMenuContextAttribute = typeSymbol.FindAttribute<ActionMenuContextAttribute>();
            AttributeData? repositoryActionAttribute = typeSymbol.FindAttribute<RepositoryActionAttribute>();
            
            if (actionMenuContextAttribute != null)
            {
                var actionMenuContextClassDescriptor = new ActionMenuContextClassDescriptor
                    {
                        Name = new ActionMenuContextAttribute((string)actionMenuContextAttribute.ConstructorArguments[0].Value!).Name!,
                    };

                projectDescriptor.ActionContextMenus.Add(actionMenuContextClassDescriptor);

                classDescriptor = actionMenuContextClassDescriptor;
            }
            else if (repositoryActionAttribute != null)
            {
                var actionMenuClassDescriptor = new ActionMenuClassDescriptor
                    {
                        Name = new RepositoryActionAttribute((string)repositoryActionAttribute.ConstructorArguments[0].Value!).Type,
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
        }
    }
    
    private static void ProcessPossibleActionContextType(Dictionary<string, string> files, Compilation compilation, Dictionary<string, KalkModuleToGenerate> mapNameToModule)
    {
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            AttributeData? moduleAttribute = typeSymbol.FindAttribute<ActionMenuContextAttribute>();
            KalkModuleToGenerate? moduleToGenerate = null;
            if (moduleAttribute != null)
            {
                GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate, mapNameToModule, files);
            }

            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                AttributeData? attr = member.FindAttribute<ActionMenuContextMemberAttribute>();
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
                    //Module = moduleToGenerate,
                };
                desc.Names.Add(name);

                if (method != null)
                {
                    desc.CSharpName = method.Name;

                    desc.IsAction = method.ReturnsVoid;
                    desc.IsFunc = !desc.IsAction;
                    var builder = new StringBuilder();
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

    private static async Task GenerateModuleSiteDocumentation2(ActionMenuContextClassDescriptor module, string siteFolder, Template template)
    {
        (string fileName, string content) = await GenerateModuleSiteDocumentationFromProjectDescription(module, template);
        await File.WriteAllTextAsync(Path.Combine(siteFolder, fileName), content);
    }

    public static async Task<(string fileName, string content)> GenerateModuleSiteDocumentationFromProjectDescription(ActionMenuContextClassDescriptor actionMenuContextClassDescriptor, Template template)
    {
        var result = await DocumentationGenerator.GetDocsContentAsyncNew(actionMenuContextClassDescriptor, template);
        var name = actionMenuContextClassDescriptor.Name.ToLowerInvariant();
        return ($"script_variables_{name}.generated.md", result);
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

    public static async Task<Template> LoadTemplateAsync(string path)
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