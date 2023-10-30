namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Broslyn;
using Microsoft.CodeAnalysis;

internal static class CompilationHelper
{
    public static async Task<Compilation> CompileAsync(string pathToSolution, string projectName)
    {
        CSharpCompilationCaptureResult compilationCaptureResult = CSharpCompilationCapture.Build(pathToSolution);
        Solution solution = compilationCaptureResult.Workspace.CurrentSolution;

        var projects = solution.Projects.ToArray();
        Project project = projects.Single(x => x.Name == projectName);

        // Make sure that doc will be parsed
        project = project.WithParseOptions(project.ParseOptions!.WithDocumentationMode(DocumentationMode.Parse));

        // Compile the project
        Compilation compilation = await project.GetCompilationAsync() ?? throw new Exception("Compilation failed");
        ValidateCompilation(compilation);
        return compilation;
    }

    private static void ValidateCompilation(Compilation compilation)
    {
        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
        Diagnostic[] errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        if (errors.Length <= 0)
        {
            return;
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