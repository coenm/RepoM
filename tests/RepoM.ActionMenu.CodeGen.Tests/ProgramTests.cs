namespace RepoM.ActionMenu.CodeGen.Tests;

using System;
using System.Threading.Tasks;
using Scriban;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ProgramTests : IClassFixture<CompiledProjectFixture>
{
    private readonly CompiledProjectFixture _fixture;

    private readonly string _pathToSolution;
    private const string PROJECT_NAME = "RepoM.ActionMenu.CodeGenDummyLibrary";


    public ProgramTests(CompiledProjectFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public async Task CompileAndExtractProjectDescription_ShouldReturn_WhenValidProject()
    {
        await Verifier.Verify(_fixture.Project);
    }

    [Fact]
    public async Task GetDocsContentAsync_ShouldReturnAsExpected()
    {
        // arrange
        Template templateDocs = await Program.LoadTemplateAsync("Templates/DocsScriptVariables.scriban-txt");
        var content = string.Empty;

        // act
        if (_fixture.Project.ActionContextMenus.Count > 0)
        {
            content = await DocumentationGenerator.GetDocsContentAsync(_fixture.Project.ActionContextMenus[0]!, templateDocs);
        }

        // assert
        await Verifier.Verify(content);
    }
}