namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System.Collections.Generic;
using FakeItEasy;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Common;
using SimpleInjector;
using Xunit;
using VerifyXunit;
using EasyTestFileXunit;
using EasyTestFile;
using VerifyTests;
using RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

[UsesEasyTestFile]
[UsesVerify]
public class IntegrationTest
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly FilesICompareSettingsService _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public IntegrationTest()
    {
        var container = new Container();
        new StatisticsPackage().RegisterServices(container);
        
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        _fileSystem = new MockFileSystem();
        container.Register<FilesICompareSettingsService>(Lifestyle.Singleton);
        container.RegisterSingleton(A.Dummy<IClock>);
        container.RegisterInstance<IAppDataPathProvider>(_appDataPathProvider);
        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterInstance<IFileSystem>(_fileSystem);

       A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("C:\\\\dir");

        container.Verify();

        _sut = container.GetInstance<FilesICompareSettingsService>();

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("yml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task LastOpenedConfiguration()
    {
        // arrange
        await _fileSystem.AddEasyFile("C:\\\\dir\\RepoM.Ordering.yaml", _testFileSettings);

        // act
        Dictionary<string, IRepositoriesComparerConfiguration> result =_sut.Configuration;

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
    [Fact]
    public async Task LastOpenedConfiguration1()
    {
        // arrange
        await _fileSystem.AddEasyFile("C:\\\\dir\\RepoM.Ordering.yaml", _testFileSettings);

        // act
        Dictionary<string, IRepositoriesComparerConfiguration> result =_sut.Configuration;

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
}