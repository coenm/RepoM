namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;

public class AzureDevopsContextTests : IntegrationActionTestBase<AzureDevOpsPackage>
{
    public AzureDevopsContextTests()
    {
        IAzureDevOpsPullRequestService azureDevOpsPullRequestService = A.Fake<IAzureDevOpsPullRequestService>();
        Container.RegisterInstance(azureDevOpsPullRequestService);

        A.CallTo(() => azureDevOpsPullRequestService.GetPullRequests(Repository, "dummy_project_id", null!)).Returns(new List<PullRequest>()
            {
                new (Guid.Empty, "test pr", "https://azure-devops.test/pr/123"),
            });
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
}