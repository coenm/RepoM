namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System;
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
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin;
using RepoM.Plugin.Statistics.PersistentConfiguration;

[UsesEasyTestFile]
[UsesVerify]
public class IntegrationTest
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly FilesCompareSettingsService _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public IntegrationTest()
    {
        var packageConfiguration = A.Fake<IPackageConfiguration>();
        A.CallTo(() => packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => packageConfiguration.LoadConfigurationAsync<StatisticsConfigV1>())
         .ReturnsLazily(() => new StatisticsConfigV1 { PersistenceBuffer = TimeSpan.FromMinutes(5), RetentionDays = 30, });
        var container = new Container();
        var package = new StatisticsPackage();
        package.RegisterServicesAsync(container, packageConfiguration).GetAwaiter().GetResult();
        
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        _fileSystem = new MockFileSystem();
        container.Register<FilesCompareSettingsService>(Lifestyle.Singleton);
        container.RegisterSingleton(A.Dummy<IClock>);
        container.RegisterInstance<IAppDataPathProvider>(_appDataPathProvider);
        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterInstance<IFileSystem>(_fileSystem);

       A.CallTo(() => _appDataPathProvider.AppDataPath).Returns("C:\\\\dir");

        container.Verify();

        _sut = container.GetInstance<FilesCompareSettingsService>();

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