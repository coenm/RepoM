namespace RepoM.Plugin.SonarCloud.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using EasyTestFileXunit;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using RepoM.Plugin.SonarCloud;

public class SonarCloudSetFavoriteV1Tests : IntegrationActionTestBase<SonarCloudPackage>
{
    private readonly ISonarCloudFavoriteService _sonarCloudService;

    public SonarCloudSetFavoriteV1Tests()
    {
        _sonarCloudService = A.Fake<ISonarCloudFavoriteService>();
        Container.RegisterInstance(_sonarCloudService);
    }

    [Fact]
    public async Task SetFavoriteScenario01()
    {
        // arrange
        A.CallTo(() => _sonarCloudService.IsInitialized).Returns(true);
        A.CallTo(() => _sonarCloudService.IsFavorite(A<string>._)).Returns(false);
        var content = await EasyTestFile.LoadAsText(TestFileSettings);
        AddRootFile(content);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result, VerifySettings);
    }
}