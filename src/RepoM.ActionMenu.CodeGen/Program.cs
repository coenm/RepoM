namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Misc;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban;

public static class Program
{
    public static async Task Main()
    {
        // var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        // var fullClassName = $"{ns}.{className}";
        
        FileSystemHelper.CheckDirectoryExists(RepoMFolders.Source);
        FileSystemHelper.CheckDirectoryExists(RepoMFolders.Documentation);
        FileSystemHelper.CheckDirectoryExists(Path.Combine(RepoMFolders.Root, ".git"));
        
        Template templateModule = await LoadTemplateAsync("Templates/ScribanModuleRegistration.scriban-cs");
        Template templateDocs = await LoadTemplateAsync("Templates/DocsScriptVariables.scriban-txt");
        Template templatePluginDocs = await LoadTemplateAsync("Templates/DocsPlugin.scriban-txt");

        Dictionary<string, string> snippetFiles = await LoadOldDocumentationSnippetFilesAsync();

        Console.WriteLine("Compiling projects ..");
        List<ProjectDescriptor> processedProjects = await CompileProjectsAsync(snippetFiles);
        Console.WriteLine(string.Empty);

        Console.WriteLine("Get all members from all projects");
        Dictionary<string, List<MemberDescriptor>> allMemberTypes = GetAllMembersFromProjects(processedProjects);
        Console.WriteLine(string.Empty);

        processedProjects.RemoveAll(p => p.ProjectName.Equals("RepoM.ActionMenu.Interface"));

        Console.WriteLine("Update member type descriptions for all projects");
        UpdateMemberTypeDescriptions(processedProjects, allMemberTypes);
        Console.WriteLine(string.Empty);

        await GenerateOutputAsync(processedProjects, templatePluginDocs, templateDocs, templateModule);
    }

    /// <summary>
    /// Copy descriptions from if (string.IsNullOrWhiteSpace(memberDescriptor.Description) && string.IsNullOrWhiteSpace(memberDescriptor.InheritDocs))
    /// </summary>
    private static void UpdateMemberTypeDescriptions(List<ProjectDescriptor> processedProjects, Dictionary<string, List<MemberDescriptor>> allMemberTypes)
    {
        foreach (ProjectDescriptor project in processedProjects)
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

                    if (!allMemberTypes.TryGetValue(className, out List<MemberDescriptor>? memberTypes))
                    {
                        throw new Exception("Cannot find Inherit docs type");
                    }

                    MemberDescriptor? matchMemberDescriptor = memberTypes.SingleOrDefault(memberType => memberType.CSharpName == typeName);
                    if (matchMemberDescriptor == null)
                    {
                        throw new Exception("Cannot find Inherit docs type");
                    }

                    memberDescriptor.Description = matchMemberDescriptor.Description;
                }
            }
        }
    }

    private static async Task GenerateOutputAsync(List<ProjectDescriptor> processedProjects, Template templatePluginDocs, Template templateDocs, Template templateModule)
    {
        await GenerateDocumentationAsync(processedProjects, templatePluginDocs, templateDocs);
        await GenerateCodeAsync(processedProjects, templateModule);
    }

    private static async Task GenerateCodeAsync(List<ProjectDescriptor> processedProjects, Template templateModule)
    {
        // Generate module registration code in c#.
        foreach (ProjectDescriptor project in processedProjects)
        {
            var fileName = Path.Combine(project.Directory, "RepoMCodeGen.generated.cs");

            if (project.ActionContextMenus.Count == 0)
            {
                FileSystemHelper.DeleteFileIfExist(fileName);
                continue;
            }

            var content = await DocumentationGenerator.GetScribanInitializersCSharpCodeAsync(project.ActionContextMenus, templateModule).ConfigureAwait(false);
            await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
        }
    }

    private static async Task GenerateDocumentationAsync(List<ProjectDescriptor> processedProjects, Template templatePluginDocs, Template templateDocs)
    {
        // Generate plugin documentation
        foreach (ProjectDescriptor project in processedProjects)
        {
            if (project.IsPlugin)
            {
                var name = project.ProjectName.ToLowerInvariant();
                var fileName = Path.Combine(RepoMFolders.DocumentationMarkDownSource, $"plugin_{name}.generated.source.md");
                var content = await DocumentationGenerator.GetPluginDocsContentAsync(project, templatePluginDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(RepoMFolders.Documentation, $"plugin_{name}.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
            else
            {
                // core
                var fileName = Path.Combine(RepoMFolders.DocumentationMarkDownSource, "repom.generated.source.md");
                var content = await DocumentationGenerator.GetPluginDocsContentAsync(project, templatePluginDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(RepoMFolders.Documentation, "repom.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
        }

        // Generate module site documentation
        foreach (ProjectDescriptor project in processedProjects)
        {
            foreach (ActionMenuContextClassDescriptor actionContextMenu in project.ActionContextMenus)
            {
                var name = actionContextMenu.Name.ToLowerInvariant();
                var fileName = Path.Combine(RepoMFolders.DocumentationMarkDownSource, $"script_variables_{name}.generated.source.md");
                var content = await DocumentationGenerator.GetDocsContentAsync(actionContextMenu, templateDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);

                fileName = Path.Combine(RepoMFolders.Documentation, $"script_variables_{name}.generated.md");
                if (!File.Exists(fileName))
                {
                    await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
                }
            }
        }
    }

    private static Dictionary<string, List<MemberDescriptor>> GetAllMembersFromProjects(List<ProjectDescriptor> processedProjects)
    {
        Dictionary<string, List<MemberDescriptor>> allMemberTypes = new();

        foreach (ProjectDescriptor project in processedProjects)
        {
            foreach (ActionMenuClassDescriptor classDescriptor in project.ActionMenus)
            {
                allMemberTypes.TryAdd(classDescriptor.FullName, []);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.ActionMenuProperties);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.Members);
            }

            foreach (ActionMenuContextClassDescriptor classDescriptor in project.ActionContextMenus)
            {
                allMemberTypes.TryAdd(classDescriptor.FullName, []);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.Members);
            }

            foreach (ModuleConfigurationClassDescriptor classDescriptor in project.ConfigurationClasses)
            {
                allMemberTypes.TryAdd(classDescriptor.FullName, []);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.Members);
            }

            foreach (ModuleConfigurationClassDescriptor classDescriptor in project.ConfigurationClasses)
            {
                allMemberTypes.TryAdd(classDescriptor.FullName, []);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.Members);
            }

            foreach (ClassDescriptor classDescriptor in project.Types)
            {
                allMemberTypes.TryAdd(classDescriptor.FullName, []);
                allMemberTypes[classDescriptor.FullName].AddRange(classDescriptor.Members);
            }
        }

        return allMemberTypes;
    }

    private static async Task<List<ProjectDescriptor>> CompileProjectsAsync(Dictionary<string, string> files)
    {
        List<ProjectDescriptor> processedProjects = new(Constants.Projects.Count);
        var compile = new CompileRepoM();
       
        foreach (var project in Constants.Projects)
        {
            var fullCsProjectFilename = Path.Combine(RepoMFolders.Source, project, $"{project}.csproj");
            Console.WriteLine($" - {fullCsProjectFilename} .. ");

            FileSystemHelper.CheckFileExists(fullCsProjectFilename);

            ProjectDescriptor projectDescriptor = await CompileAndExtractProjectDescriptionAsync(compile, fullCsProjectFilename, project, files);
            processedProjects.Add(projectDescriptor);
            Console.WriteLine("    done");
        }

        return processedProjects;
    }

    public static async Task<ProjectDescriptor> CompileAndExtractProjectDescriptionAsync(CompileRepoM compile, string pathToSolution, string project, IDictionary<string, string> files)
    {
        Compilation compilation = await compile.CompileAsync(pathToSolution, project).ConfigureAwait(false);

        var projectDescriptor = new ProjectDescriptor
        {
            AssemblyName = compilation.AssemblyName ?? throw new Exception("Could not determine AssemblyName"),
            ProjectName = project,
            FullFilename = pathToSolution,
            Directory = Path.GetDirectoryName(pathToSolution) ?? throw new Exception("Could not determine Directory"),
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
            AttributeData? moduleConfigurationAttribute = typeSymbol.FindAttribute<ModuleConfigurationAttribute>();
            
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
            else if (moduleConfigurationAttribute != null)
            {
                var actionMenuClassDescriptor = new ModuleConfigurationClassDescriptor
                {
                    IsObsolete = false,
                };
                projectDescriptor.ConfigurationClasses.Add(actionMenuClassDescriptor);

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

    private static async Task<Dictionary<string, string>> LoadOldDocumentationSnippetFilesAsync()
    {
        var path = Path.Combine(RepoMFolders.DocumentationOld, "snippets");
        var fileNames = Directory.GetFiles(path);

        var result = new Dictionary<string, string>(fileNames.Length);

        foreach (var file in fileNames)
        {
            var fi = new FileInfo(file);
            var fileContent = await File.ReadAllTextAsync(file);
            result.Add(fi.Name, fileContent);
        }

        return result;
    }
}