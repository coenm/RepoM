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
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban;

public static class Program
{
    public static async Task Main()
    {
        // var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        // var fullClassName = $"{ns}.{className}";
        
        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "src");
        var docsFolder = Path.Combine(rootFolder, "docs_new");

        FileSystemHelper.CheckDirectory(srcFolder);
        FileSystemHelper.CheckDirectory(docsFolder);
        FileSystemHelper.CheckDirectory(Path.Combine(rootFolder, ".git"));

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

        foreach (var project in projects)
        {
            var pathToSolution = Path.Combine(srcFolder, project, $"{project}.csproj");
            FileSystemHelper.CheckFile(pathToSolution);

            ProjectDescriptor projectDescriptor = await CompileAndExtractProjectDescription(pathToSolution, project, files);
            
            processedProjects.Add(project, projectDescriptor);
        }

        // Generate module site documentation
        foreach ((var projectName, ProjectDescriptor? project) in processedProjects)
        {
            foreach (ActionMenuContextClassDescriptor actionContextMenu in project.ActionContextMenus)
            {
                var name = actionContextMenu.Name.ToLowerInvariant();
                var fileName = Path.Combine(docsFolder, $"script_variables_{name}.generated.md");
                var content = await DocumentationGenerator.GetDocsContentAsync(actionContextMenu, templateDocs).ConfigureAwait(false);
                await File.WriteAllTextAsync(fileName, content).ConfigureAwait(false);
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

    public static async Task<ProjectDescriptor> CompileAndExtractProjectDescription(string pathToSolution, string project, IDictionary<string, string> files)
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

        return projectDescriptor;
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