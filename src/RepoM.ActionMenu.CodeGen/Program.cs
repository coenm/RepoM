namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Misc;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.Core.TestLib.Utils;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban;

public static class Program
{
    internal static readonly Dictionary<string, TypeInfoDescriptor> TypeInfos = new()
        {
            {
                typeof(Interface.YamlModel.Templating.Text).FullName!,
                new TypeInfoDescriptor(nameof(Text), typeof(Interface.YamlModel.Templating.Text).FullName!)
                    {
                        Link = "repository_action_types.md#text",
                    }
            },
            {
                typeof(Interface.YamlModel.Templating.Predicate).FullName!,
                new TypeInfoDescriptor(nameof(Interface.YamlModel.Templating.Predicate), typeof(Interface.YamlModel.Templating.Predicate).FullName!)
                    {
                        Link = "repository_action_types.md#predicate",
                    }
            },
            {
                typeof(Interface.YamlModel.ActionMenus.Context).FullName!,
                new TypeInfoDescriptor(nameof(Interface.YamlModel.ActionMenus.Context), typeof(Interface.YamlModel.ActionMenus.Context).FullName!)
                    {
                        Link = "repository_action_types.md#context",
                    }
            },
            {
                typeof(Interface.YamlModel.ActionMenus.Context).FullName! + "?",
                new TypeInfoDescriptor(nameof(Interface.YamlModel.ActionMenus.Context), typeof(Interface.YamlModel.ActionMenus.Context).FullName! + "?")
                    {
                        Link = "repository_action_types.md#context",
                    }
            },
        };

    public static async Task Main()
    {
        // var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        // var fullClassName = $"{ns}.{className}";
        var compile = new CompileRepoM();
        var rootFolder = ThisProjectAssembly.Info.GetSolutionDirectory();
        var srcFolder = Path.Combine(rootFolder, "src");
        var docsFolder = Path.Combine(rootFolder, "docs");
        var docsFolderSource = Path.Combine(docsFolder, "mdsource");

        FileSystemHelper.CheckDirectory(srcFolder);
        FileSystemHelper.CheckDirectory(docsFolder);
        FileSystemHelper.CheckDirectory(Path.Combine(rootFolder, ".git"));

        var projects = new List<string>
            {
                "RepoM.ActionMenu.Interface", // this is for the description of the interface types and its members.
                "RepoM.ActionMenu.Core",
                
                "RepoM.Plugin.AzureDevOps",
                "RepoM.Plugin.Clipboard",
                "RepoM.Plugin.Heidi",
                "RepoM.Plugin.LuceneQueryParser",
                "RepoM.Plugin.SonarCloud",
                "RepoM.Plugin.Statistics",
                "RepoM.Plugin.WebBrowser",
                "RepoM.Plugin.WindowsExplorerGitInfo",
            };

        Template templateModule = await LoadTemplateAsync("Templates/ScribanModuleRegistration.scriban-cs");
        Template templateDocs = await LoadTemplateAsync("Templates/DocsScriptVariables.scriban-txt");
        Template templatePluginDocs = await LoadTemplateAsync("Templates/DocsPlugin.scriban-txt");

        Dictionary<string, string> files = await LoadFilesAsync(rootFolder);
        
        var processedProjects = new Dictionary<string, ProjectDescriptor>();

        foreach (var project in projects)
        {
            var fullCsProjectFilename = Path.Combine(srcFolder, project, $"{project}.csproj");
            FileSystemHelper.CheckFile(fullCsProjectFilename);

            ProjectDescriptor projectDescriptor = await CompileAndExtractProjectDescription(compile, fullCsProjectFilename, project, files);
            processedProjects.Add(project, projectDescriptor);
        }

        Dictionary<string, List<MemberDescriptor>> _allTypes2 = new();
        
        foreach ((var projectName, ProjectDescriptor project) in processedProjects)
        {
            foreach (var classDescriptor in project.ActionMenus)
            {
                if (!_allTypes2.ContainsKey(classDescriptor.Namespace + "." + classDescriptor.ClassName))
                {
                    _allTypes2.Add(classDescriptor.Namespace + "." + classDescriptor.ClassName, new List<MemberDescriptor>());
                }

                foreach (var memberDescriptor in classDescriptor.ActionMenuProperties)
                {
                    _allTypes2[classDescriptor.Namespace + "." + classDescriptor.ClassName].Add(memberDescriptor);
                }
                foreach (var memberDescriptor in classDescriptor.Members)
                {
                    _allTypes2[classDescriptor.Namespace + "." + classDescriptor.ClassName].Add(memberDescriptor);
                }
            }

            foreach (var classDescriptor in project.ActionContextMenus)
            {
                if (!_allTypes2.ContainsKey(classDescriptor.Namespace + "." + classDescriptor.ClassName))
                {
                    _allTypes2.Add(classDescriptor.Namespace + "." + classDescriptor.ClassName, new List<MemberDescriptor>());
                }
                foreach (var memberDescriptor in classDescriptor.Members)
                {
                    _allTypes2[classDescriptor.Namespace + "." + classDescriptor.ClassName].Add(memberDescriptor);
                }
            }

            foreach (var classDescriptor in project.Types)
            {
                if (!_allTypes2.ContainsKey(classDescriptor.Namespace + "." + classDescriptor.ClassName))
                {
                    _allTypes2.Add(classDescriptor.Namespace + "." + classDescriptor.ClassName, new List<MemberDescriptor>());
                }
                foreach (var memberDescriptor in classDescriptor.Members)
                {
                    _allTypes2[classDescriptor.Namespace + "." + classDescriptor.ClassName].Add(memberDescriptor);
                }
            }
        }

        processedProjects.Remove("RepoM.ActionMenu.Interface");

        // Copy descriptions from if (string.IsNullOrWhiteSpace(memberDescriptor.Description) && string.IsNullOrWhiteSpace(memberDescriptor.InheritDocs))
        foreach ((var projectName, ProjectDescriptor project) in processedProjects)
        {
            foreach (ActionMenuClassDescriptor classDescriptor in project.ActionMenus)
            {
                foreach (ActionMenuMemberDescriptor memberDescriptor in classDescriptor.ActionMenuProperties)
                {
                    if (!string.IsNullOrWhiteSpace(memberDescriptor.Description) || string.IsNullOrWhiteSpace(memberDescriptor.InheritDocs))
                    {
                        continue;
                    }

                    var index = memberDescriptor.InheritDocs.LastIndexOf('.');
                    var className = memberDescriptor.InheritDocs[..index];
                    var typeName = memberDescriptor.InheritDocs[(index + 1)..];

                    if (_allTypes2.TryGetValue(className, out List<MemberDescriptor>? xxx))
                    {
                        MemberDescriptor? matchMemberDescriptor = xxx.SingleOrDefault(x => x.CSharpName == typeName);
                        if (matchMemberDescriptor != null)
                        {
                            memberDescriptor.Description = matchMemberDescriptor.Description;
                        }
                        else
                        {
                            Console.WriteLine("InheritDocs not found");
                        }
                    }
                    else
                    {
                        Console.WriteLine("InheritDocs not found");
                    }
                }
            }
        }


        // Generate plugin documentation
        foreach ((var projectName, ProjectDescriptor? project) in processedProjects)
        {
            if (project.IsPlugin)
            {
                var name = project.ProjectName.ToLowerInvariant();
                var fileName = Path.Combine(docsFolderSource, $"plugin_{name}.generated.source.md");
                var content = await DocumentationGenerator.GetPluginDocsContentAsync(project, templatePluginDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(docsFolder, $"plugin_{name}.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
            else
            {
                // core
                var fileName = Path.Combine(docsFolderSource, "repom.generated.source.md");
                var content = await DocumentationGenerator.GetPluginDocsContentAsync(project, templatePluginDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(docsFolder, "repom.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
        }

        // Generate module site documentation
        foreach ((var projectName, ProjectDescriptor? project) in processedProjects)
        {
            foreach (ActionMenuContextClassDescriptor actionContextMenu in project.ActionContextMenus)
            {
                var name = actionContextMenu.Name.ToLowerInvariant();
                var fileName = Path.Combine(docsFolderSource, $"script_variables_{name}.generated.source.md");
                var content = await DocumentationGenerator.GetDocsContentAsync(actionContextMenu, templateDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(docsFolder, $"script_variables_{name}.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
        }

        // Generate module registration code in c#.
        foreach ((var projectName, ProjectDescriptor? project) in processedProjects)
        {
            var fileName = Path.Combine(srcFolder, projectName, "RepoMCodeGen.generated.cs");

            if (project.ActionContextMenus.Count == 0)
            {
                FileSystemHelper.DeleteFileIsExist(fileName);
                continue;
            }

            var content = await DocumentationGenerator.GetScribanInitializersCSharpCodeAsync(project.ActionContextMenus, templateModule).ConfigureAwait(false);
            await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
        }
    }

    public static async Task<ProjectDescriptor> CompileAndExtractProjectDescription(CompileRepoM compile, string pathToSolution, string project, IDictionary<string, string> files)
    {
        Compilation compilation = await compile.CompileAsync(pathToSolution, project).ConfigureAwait(false);

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
                (assemblyAttribute.ConstructorArguments[1].Value as string)!,
                (assemblyAttribute.ConstructorArguments[2].Value as string)!);
            projectDescriptor.SetPackageInformation(pa);
        }

        ProcessProject(compilation, projectDescriptor, files);

        return projectDescriptor;
    }

    private static void ProcessProject(Compilation compilation, ProjectDescriptor projectDescriptor, IDictionary<string, string> files)
    {
        foreach (ITypeSymbol typeSymbol in compilation.GetTypes())
        {
            ProcessMembersVisitor memberVisitor = new(typeSymbol, files);
            DocsClassVisitor docsClassVisitor = new(typeSymbol, files);

            ClassDescriptor classDescriptor;

            AttributeData? obsoleteAttribute = typeSymbol.FindAttribute<ObsoleteAttribute>();
            AttributeData? actionMenuContextAttribute = typeSymbol.FindAttribute<ActionMenuContextAttribute>();
            AttributeData? repositoryActionAttribute = typeSymbol.FindAttribute<RepositoryActionAttribute>();
            
            if (actionMenuContextAttribute != null && obsoleteAttribute == null)
            {
                var actionMenuContextClassDescriptor = new ActionMenuContextClassDescriptor
                    {
                        Name = new ActionMenuContextAttribute((string)actionMenuContextAttribute.ConstructorArguments[0].Value!).Name!,
                    };

                projectDescriptor.ActionContextMenus.Add(actionMenuContextClassDescriptor);

                classDescriptor = actionMenuContextClassDescriptor;
            }
            else if (repositoryActionAttribute != null && obsoleteAttribute == null)
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

    private static async Task<Dictionary<string, string>> LoadFilesAsync(string root)
    {
        var path = Path.Combine(root, "docs_old", "snippets");
        var fileNames = Directory.GetFiles(path);

        var result = new Dictionary<string, string>(fileNames.Length);

        foreach (var file in fileNames)
        {
            var f = new FileInfo(file);
            var fileContent = await File.ReadAllTextAsync(file);
            result.Add(f.Name, fileContent);
        }

        return result;
    }
}