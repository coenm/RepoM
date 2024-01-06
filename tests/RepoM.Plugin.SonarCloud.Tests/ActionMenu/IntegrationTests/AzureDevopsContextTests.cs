namespace RepoM.Plugin.SonarCloud.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.SonarCloud;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class SonarCloudContextTests : IntegrationActionTestBase<SonarCloudPackage>
{
    private readonly ISonarCloudFavoriteService _sonarCloudService;

    public SonarCloudContextTests()
    {
        _sonarCloudService = A.Fake<ISonarCloudFavoriteService>();
        Container.RegisterInstance(_sonarCloudService);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Context_GetPullRequests(bool favorite)
    {
        // arrange
        A.CallTo(() => _sonarCloudService.IsFavorite("dummy_project_id")).Returns(favorite);

        const string YAML =
            """
            context:
            - type: evaluate-script@1
              content: |-
                is_sonar_favorite = sonarcloud.is_favorite("dummy_project_id");

            action-menu:
            - type: just-text@1
              name: 'is_sonar_favorite: [{{ is_sonar_favorite }}];'
            """;
        AddRootFile(YAML);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        var singleAction = result.Single() as UserInterfaceRepositoryAction;
        singleAction.Should().NotBeNull();
        singleAction!.Name.Should().Be($"is_sonar_favorite: [{favorite.ToString().ToLower()}];");
    }

    // /// <summary>
    // /// Validate file to contain valid action menu yaml.
    // /// </summary>
    // [Fact]
    // [Documentation]
    // public async Task Context_GetPullRequests_Documentation()
    // {
    //     // arrange
    //     var yaml = await DocumentationGeneration.LoadYamlFileAsync("azure_devops.get_pull_requests.actionmenu.yaml");
    //     AddRootFile(yaml);
    //
    //     // act
    //     IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();
    //     
    //     // assert
    //     await Verifier.Verify(result).ScrubMembersWithType<IRepository>();
    // }
}