namespace RepoM.ActionMenu.CodeGen.Misc;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Broslyn;
using Microsoft.CodeAnalysis;

public class CompileRepoM
{
    private readonly ConcurrentDictionary<string, Tuple<Project, Compilation>> _compilations = new();

    public Dictionary<string, Tuple<Project, Compilation>> Store => _compilations.ToDictionary(x => x.Key, x => x.Value);

    public async Task<Compilation> CompileAsync(string pathToSolution, string projectName)
    {
        CSharpCompilationCaptureResult compilationCaptureResult = CSharpCompilationCapture.Build(pathToSolution);
        Solution solution = compilationCaptureResult.Workspace.CurrentSolution;

        Project[] projects = solution.Projects.ToArray();

        foreach (Project p in projects)
        {
            if (_compilations.ContainsKey(p.Name))
            {
                continue;
            }

            Project project = p.WithParseOptions(p.ParseOptions!.WithDocumentationMode(DocumentationMode.Parse));

            // Compile the project
            Compilation compilation = await project.GetCompilationAsync() ?? throw new Exception("Compilation failed");
            ValidateCompilation(compilation);

            _compilations.AddOrUpdate(p.Name, _ => new Tuple<Project, Compilation>(project, compilation), (_, __) => new Tuple<Project, Compilation>(project, compilation));
        }

        if (_compilations.TryGetValue(projectName, out Tuple<Project, Compilation>? tuple))
        {
            return tuple.Item2;
        }

        throw new Exception("Not found");
    }

    private static void ValidateCompilation(Compilation compilation)
    {
        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
        Diagnostic[] errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        if (errors.Length <= 0)
        {
            return;
        }

        if ("RepoM.Api".Equals(compilation.AssemblyName))
        {
            if (errors is [{ Id: "CS8795", },])
            {
                return;
            }
        }

        if ("RepoM.Plugin.AzureDevOps".Equals(compilation.AssemblyName))
        {
            if (errors is [{ Id: "CS8795", },])
            {
                return;
            }
        }

        if ("RepoM.Plugin.EverythingFileSearch".Equals(compilation.AssemblyName))
        {
            if (errors.Length == 8)
            {
                return;
            }
        }

        Console.WriteLine("Compilation errors:");
        foreach (Diagnostic error in errors)
        {
            Console.WriteLine(error);
        }

        Console.WriteLine("Error, Exiting.");
        Environment.Exit(1);
        throw new Exception("Compilation error");
    }
}