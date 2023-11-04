namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.IntegrationTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class FilesContextTests : IntegrationActionTestBase
{
    public FilesContextTests()
    {
        var rootPath = Path.Combine("C:", "Repositories", "work", "RepoX");

        FileSystem.AddDirectory(Path.Combine(rootPath, "src"));
        FileSystem.AddFile(Path.Combine(rootPath, "my-solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "test solution.sln"), new MockFileData("dummy"));
        FileSystem.AddFile(Path.Combine(rootPath, "src", "dummy.txt"), new MockFileData("dummy"));


        //     IAzureDevOpsPullRequestService azureDevOpsPullRequestService = A.Fake<IAzureDevOpsPullRequestService>();
        //     Container.RegisterInstance(azureDevOpsPullRequestService);
        //
        //     A.CallTo(() => azureDevOpsPullRequestService.GetPullRequests(Repository, "dummy_project_id", null!)).Returns(new List<PullRequest>()
        //         {
        //             new (Guid.Empty, "test pr", "https://azure-devops.test/pr/123"),
        //         });
        //     A.CallTo(() => azureDevOpsPullRequestService.GetPullRequests(Repository, "805ACF64-0F06-47EC-96BF-E830895E2740", null)).Returns(_prs);
    }

    /// <summary>
    /// Validate file to contain valid action menu yaml.
    /// </summary>
    [Fact]
    [Documentation]
    public async Task Context_FindFiles_Documentation()
    {
        // arrange
        var yaml = await DocumentationGeneration.LoadYamlFileAsync("file.find_files.actionmenu.yaml");
        AddRootFile(yaml);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();
        
        // assert
        await Verifier.Verify(result).ScrubMembersWithType<IRepository>();
    }
}