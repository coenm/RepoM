namespace RepoM.ActionMenu.CodeGen.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RepoM.ActionMenu.CodeGen.Models.New;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ProgramTests
{
    private const string PROJECT_NAME = "RepoM.ActionMenu.CodeGenDummyLibrary";
    
    [Fact]
    public async Task CompileAndExtractProjectDescription_ShouldReturn_WhenValidProject()
    {
        // arrange
        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "tests");
        var pathToSolution = Path.Combine(srcFolder, PROJECT_NAME, $"{PROJECT_NAME}.csproj");

        // act
        (_, ProjectDescriptor projectDescriptor) = await Program.CompileAndExtractProjectDescription(pathToSolution, PROJECT_NAME, new Dictionary<string, string>());

        // assert
        await Verifier.Verify(projectDescriptor);
    }
}