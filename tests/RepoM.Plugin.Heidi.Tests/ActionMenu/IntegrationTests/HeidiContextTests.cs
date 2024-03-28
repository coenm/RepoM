namespace RepoM.Plugin.Heidi.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FakeItEasy;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Tests;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

[UsesEasyTestFile]
public class HeidiContextTests : IntegrationActionTestBase<HeidiPackage>
{
    private readonly EasyTestFileSettings _testFileSettings;

    public HeidiContextTests()
    {
        IHeidiConfigurationService service = A.Fake<IHeidiConfigurationService>();
        Container.RegisterInstance(service);

        A.CallTo(() => service.GetByRepository(Repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new ("cc", 5, [], "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1)),
                 new ("bb", 1, [ "Test", "Dev", ], "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2)),
                 new ("aa", 5, [ "Dev", ], "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(3)),
             });

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseExtension("yaml");
    }

    /// <summary>
    /// Validate file to contain valid action menu yaml.
    /// </summary>
    [Fact]
    [Documentation]
    public async Task Context_GetDatabases_Documentation()
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