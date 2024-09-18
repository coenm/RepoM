namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTestFileXunit;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Fetch;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class GitFetchV1Tests : IntegrationActionTestBase
{
    [Theory]
    [InlineData(RepositoryActionGitFetchV1.EXAMPLE_1)]
    [Documentation]
    public async Task Documentation(string name)
    {
        // arrange
        TestFileSettings.SetTestFileNameSuffix(name);
        var yaml = await EasyTestFile.LoadAsText(TestFileSettings);
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result, VerifySettings).UseParameters(name);
    }
}