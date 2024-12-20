namespace RepoM.Plugin.Heidi.Tests;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Plugin.Heidi.PersistentConfiguration;
using SimpleInjector;
using Xunit;

public class HeidiPackageTest
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;

    public HeidiPackageTest()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _container = new Container();

        var heidiConfigV1 = new HeidiConfigV1
        {
            ConfigPath = "C:\\Config\\Path\\Test\\",
            ConfigFilename = "portable.heidi.test.txt",
            ExecutableFilename = "TestHeidiSQL.exe",
        };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<HeidiConfigV1>()).ReturnsLazily(() => heidiConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<HeidiConfigV1>._, 1)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new HeidiPackage();

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
        var sut = new HeidiPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<HeidiConfigV1>._, 1)).MustHaveHappenedOnceExactly();

        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task RegisterServices_ShouldCopyExistingAppSettingsConfig_WhenNoCurrentCorrectConfig(int? version)
    {
        // arrange
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(version));
        RegisterExternals(_container);
        var sut = new HeidiPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<HeidiConfigV1>._, 1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new HeidiPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);
        Action act = () => _container.Verify(VerificationOption.VerifyAndDiagnose);

        // assert
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterSingleton(A.Dummy<IFileSystem>);
    }
}