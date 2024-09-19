namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class BrowseRepositoryV1Tests : IntegrationActionTestBase
{
    [Theory]
    [InlineData(RepositoryActionBrowseRepositoryV1.EXAMPLE_1)]
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
        yaml.Should().Contain($"# begin-snippet: {name}");
        yaml.Should().Contain($"# end-snippet");
        await Verifier.Verify(result, VerifySettings).UseParameters(name);
    }
}