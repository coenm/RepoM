namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.IO;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using Xunit.Categories;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.ForEach;

public class ForEachV1Tests: IntegrationActionTestBase
{
    private readonly EasyTestFileSettings _testFileSettings;

    public ForEachV1Tests()
    {
        var rootPath = Path.Combine("C:", "Repositories", "work", "RepoM");

        FileSystem.AddDirectory(Path.Combine(rootPath, "src"));
        FileSystem.AddFile(Path.Combine(rootPath, "my-solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "test solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "dummy.txt"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, ".editorconfig"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "tests", "ProjA.Tests.csproj"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "tests", "ProjB.Tests.csproj"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "tests", "ProjC.Tests.csproj"), new MockFileData("dummy"));

        _testFileSettings = new EasyTestFileSettings(TestFileSettings);
    }

    [Theory]
    [InlineData(RepositoryActionForEachV1.EXAMPLE_1)]
    [InlineData(RepositoryActionForEachV1.EXAMPLE_2)]
    [Documentation]
    public async Task Documentation(string name)
    {
        // arrange
        _testFileSettings.SetTestFileNameSuffix(name);
        var yaml = await EasyTestFile.LoadAsText(_testFileSettings);
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result, VerifySettings).UseParameters(name);
    }
}