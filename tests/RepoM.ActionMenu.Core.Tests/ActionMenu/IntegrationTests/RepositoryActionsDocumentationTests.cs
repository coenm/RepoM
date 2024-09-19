namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Command;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Executable;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Folder;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.ForEach;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Checkout;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Fetch;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Pull;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Push;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Ignore;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.JustText;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Pin;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Separator;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Url;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class RepositoryActionsDocumentationTests : IntegrationActionTestBase
{
    public RepositoryActionsDocumentationTests()
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

        TestFileSettings.UseDirectory("Docs");
        VerifySettings.UseDirectory("Docs");
    }

    [Theory]
    [InlineData(RepositoryActionBrowseRepositoryV1.EXAMPLE_1)]
    [InlineData(RepositoryActionExecutableV1.EXAMPLE_1)]
    [InlineData(RepositoryActionFolderV1.EXAMPLE_1)]
    [InlineData(RepositoryActionGitCheckoutV1.EXAMPLE_1)]
    [InlineData(RepositoryActionGitFetchV1.EXAMPLE_1)]
    [InlineData(RepositoryActionGitPullV1.EXAMPLE_1)]
    [InlineData(RepositoryActionGitPushV1.EXAMPLE_1)]
    [InlineData(RepositoryActionIgnoreV1.EXAMPLE_1)]
    [InlineData(RepositoryActionPinV1.EXAMPLE_1)]
    [InlineData(RepositoryActionForEachV1.EXAMPLE_1)]
    [InlineData(RepositoryActionForEachV1.EXAMPLE_2)]
    [InlineData(RepositoryActionCommandV1.EXAMPLE_1)]
    [InlineData(RepositoryActionJustTextV1.EXAMPLE_1)]
    [InlineData(RepositoryActionSeparatorV1.EXAMPLE_1)]
    [InlineData(RepositoryActionUrlV1.EXAMPLE_1)]
    [Documentation]
    public async Task Documentation(string name)
    {
        // arrange
        TestFileSettings.UseFileName(name);
        var yaml = await EasyTestFile.LoadAsText(TestFileSettings);
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        yaml.Should().Contain($"# begin-snippet: {name}");
        yaml.Should().Contain($"# end-snippet");
        await Verifier.Verify(result, VerifySettings).UseFileName(name);
    }
}