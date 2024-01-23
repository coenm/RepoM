namespace RepoM.Plugin.AzureDevOps.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin;
using RepoM.Plugin.AzureDevOps.PersistentConfiguration;
using SimpleInjector;
using Xunit;

public class AzureDevOpsPackageTests
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;
    private readonly IAppSettingsService _appSettingsService;

    public AzureDevOpsPackageTests()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _appSettingsService = A.Fake<IAppSettingsService>();
        _container = new Container();

        var azureDevopsConfigV1 = new AzureDevopsConfigV1
            {
                PersonalAccessToken = "PAT",
                BaseUrl = "https://dev.azure.com/MyOrg",
            };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV1>()).ReturnsLazily(() => azureDevopsConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new AzureDevOpsPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task RegisterServices_ShouldPersistNewConfig_WhenVersionIsNotCorrect(int? version)
    {
        // arrange
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(version));
        RegisterExternals(_container);
        var sut = new AzureDevOpsPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).MustHaveHappenedOnceExactly();

        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }
    
    [Fact]
    public async Task RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new AzureDevOpsPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        Assert.Throws<InvalidOperationException>(() => _container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task RegisterServices_ShouldPersistConfigWhenNotCorrectVersion(int? version)
    {
        // arrange
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(version));
        RegisterExternals(_container);
        var sut = new AzureDevOpsPackage();
      
        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).MustHaveHappenedOnceExactly();
    }

    private void RegisterExternals(Container container)
    {
        container.RegisterInstance(_appSettingsService);
        container.RegisterSingleton(A.Dummy<ILogger>);
    }
}