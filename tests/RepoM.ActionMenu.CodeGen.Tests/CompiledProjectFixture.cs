namespace RepoM.ActionMenu.CodeGen.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.CodeGen.Models;
using Xunit;

[UsedImplicitly]
public class CompiledProjectFixture : IAsyncLifetime
{
    private ProjectDescriptor? _project;
    private readonly string _pathToSolution;
    private const string PROJECT_NAME = "RepoM.ActionMenu.CodeGenDummyLibrary";
    public CompiledProjectFixture()
    {
        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "tests");
        _pathToSolution = Path.Combine(srcFolder, PROJECT_NAME, $"{PROJECT_NAME}.csproj");
    }

    public ProjectDescriptor Project => _project ?? throw new Exception("Project is still null");

    public async Task InitializeAsync()
    {
        _project = await Program.CompileAndExtractProjectDescription(
            _pathToSolution,
            PROJECT_NAME,
            new Dictionary<string, string>());
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}