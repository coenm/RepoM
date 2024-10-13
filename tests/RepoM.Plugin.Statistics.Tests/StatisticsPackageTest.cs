namespace RepoM.Plugin.Statistics.Tests;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Statistics.PersistentConfiguration;
using SimpleInjector;
using Xunit;

public class StatisticsPackageTest
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;

    public StatisticsPackageTest()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _container = new Container();

        var statisticsConfigV1 = new StatisticsConfigV1
            {
                PersistenceBuffer = TimeSpan.FromMinutes(15),
                RetentionDays = 50,
            };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<StatisticsConfigV1>()).ReturnsLazily(() => statisticsConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<StatisticsConfigV1>._, 1)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new StatisticsPackage();

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
        var sut = new StatisticsPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<StatisticsConfigV1>._, 1)).MustHaveHappenedOnceExactly();

        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public async Task RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new StatisticsPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);
        Action act = () => _container.Verify(VerificationOption.VerifyAndDiagnose);

        // assert
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IClock>);
        container.RegisterSingleton(A.Dummy<IAppDataPathProvider>);
        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterSingleton(A.Dummy<IFileSystem>);
    }
}