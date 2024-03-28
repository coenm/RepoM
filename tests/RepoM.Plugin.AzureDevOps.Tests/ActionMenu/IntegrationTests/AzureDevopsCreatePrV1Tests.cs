namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using EasyTestFileXunit;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Plugin.AzureDevOps.Internal;
using VerifyXunit;
using Xunit;

public class AzureDevopsCreatePrV1Tests : IntegrationActionTestBase<AzureDevOpsPackage>
{
    public AzureDevopsCreatePrV1Tests()
    {
        IAzureDevOpsPullRequestService azureDevOpsPullRequestService = A.Fake<IAzureDevOpsPullRequestService>();
        Container.RegisterInstance(azureDevOpsPullRequestService);
    }

    [Fact]
    public async Task CreatePullRequestScenario01()
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(TestFileSettings);
        AddRootFile(content);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result, VerifySettings);
    }
}