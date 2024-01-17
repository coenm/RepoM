namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.Internal;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

[UsesEasyTestFile]
public class AzureDevopsContextTests : IntegrationActionTestBase<AzureDevOpsPackage>
{
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly List<PullRequest> _prs = new()
        {
            new(Guid.Parse("b1a0619a-cb69-4bf6-9b97-6c62481d9bff"), "some pr1", "https://my-url/pr1"),
            new(Guid.Parse("f99e85ee-2c23-414b-8804-6a6c34f8c349"), "other pr - bug", "https://my-url/pr3"),
        };

    public AzureDevopsContextTests()
    {
        IAzureDevOpsPullRequestService azureDevOpsPullRequestService = A.Fake<IAzureDevOpsPullRequestService>();
        Container.RegisterInstance(azureDevOpsPullRequestService);

        A.CallTo(() => azureDevOpsPullRequestService.GetPullRequests(Repository, "dummy_project_id", null!)).Returns(new List<PullRequest>()
            {
                new (Guid.Empty, "test pr", "https://azure-devops.test/pr/123"),
            });
        A.CallTo(() => azureDevOpsPullRequestService.GetPullRequests(Repository, "805ACF64-0F06-47EC-96BF-E830895E2740", null)).Returns(_prs);

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseExtension("yaml");
    }

    [Fact]
    public async Task Context_GetPullRequests()
    {
        // arrange
        const string YAML =
            """
            context:
            - type: evaluate-script@1
              content: |-
                prs = azure_devops.get_pull_requests("dummy_project_id");
                first_pr = array.first prs;

            action-menu:
            - type: just-text@1
              name: 'pr count: [{{ array.size prs }}]; url: [{{ first_pr.url }}]; name: [{{ first_pr.name  }}];'
            """;
        AddRootFile(YAML);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        var singleAction = result.Single() as UserInterfaceRepositoryAction;
        singleAction.Should().NotBeNull();
        singleAction!.Name.Should().Be("pr count: [1]; url: [https://azure-devops.test/pr/123]; name: [test pr];");
    }

    /// <summary>
    /// Validate file to contain valid action menu yaml.
    /// </summary>
    [Fact]
    [Documentation]
    public async Task Context_GetPullRequests_Documentation()
    {
        // arrange
        var yaml = await EasyTestFile.LoadAsText(_testFileSettings);
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();
        
        // assert
        await Verifier.Verify(result).ScrubMembersWithType<IRepository>();
    }
}