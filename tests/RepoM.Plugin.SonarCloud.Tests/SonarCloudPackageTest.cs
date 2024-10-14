namespace RepoM.Plugin.SonarCloud.Tests;

using System;
using FakeItEasy;
using RepoM.Api.Common;
using SimpleInjector;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Plugin.SonarCloud.PersistentConfiguration;
using FluentAssertions;

public class SonarCloudPackageTest
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;
    private readonly IAppSettingsService _appSettingsService;

    public SonarCloudPackageTest()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _appSettingsService = A.Fake<IAppSettingsService>();
        _container = new Container();

        var sonarCloudConfigV1 = new SonarCloudConfigV1
            {
                PersonalAccessToken = "PATx",
                BaseUrl = "https://sonarcloud.io",
            };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<SonarCloudConfigV1>()).ReturnsLazily(() => sonarCloudConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<SonarCloudConfigV1>._, 1)).Returns(Task.CompletedTask);
    }
   
    [Fact]
    public async Task RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new SonarCloudPackage();

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
        var sut = new SonarCloudPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<SonarCloudConfigV1>._, 1)).MustHaveHappenedOnceExactly();

        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public async Task RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new SonarCloudPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);
        Action act = () => _container.Verify(VerificationOption.VerifyAndDiagnose);

        // assert
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    private void RegisterExternals(Container container)
    {
        container.RegisterInstance(_appSettingsService);
        container.RegisterSingleton(A.Dummy<ILogger>);
    }
}