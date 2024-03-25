namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class FilesContextTests : IntegrationActionTestBase
{
    private readonly EasyTestFileSettings _testFileSettings;

    public FilesContextTests()
    {
        var rootPath = Path.Combine("C:", "Repositories", "work", "RepoX");

        FileSystem.AddDirectory(Path.Combine(rootPath, "src"));
        FileSystem.AddFile(Path.Combine(rootPath, "my-solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "test solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "dummy.txt"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, ".editorconfig"), new MockFileData("dummy"));

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseExtension("yaml");
    }
    
    /// <summary>
    /// Validate file to contain valid action menu yaml.
    /// </summary>
    [Theory]
    [InlineData(nameof(FileFunctions.FindFiles))]
    [InlineData(nameof(FileFunctions.FileExists))]
    [InlineData(nameof(FileFunctions.DirectoryExists))]
    [Documentation]
    public async Task Context_Documentation(string name)
    {
        // arrange
        _testFileSettings.SetTestFileNameSuffix(name);
        var yaml = await EasyTestFile.LoadAsText(_testFileSettings);
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result).ScrubMembersWithType<IRepository>().UseParameters(name);
    }
}