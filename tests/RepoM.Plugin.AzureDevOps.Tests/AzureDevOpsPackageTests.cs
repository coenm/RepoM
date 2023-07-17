namespace RepoM.Plugin.AzureDevOps.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.AzureDevOps.PersistentConfiguration;
using SimpleInjector;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class AzureDevOpsPackageTests
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;
    private readonly AzureDevopsConfigV1? _azureDevopsConfigV1;
    private IAppSettingsService _appSettingsService;

    public AzureDevOpsPackageTests()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _appSettingsService = A.Fake<IAppSettingsService>();
        _container = new Container();

        _azureDevopsConfigV1 = new AzureDevopsConfigV1
            {
                PersonalAccessToken = "PAT",
                BaseUrl = "https://dev.azure.com/MyOrg",
            };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV1>()).ReturnsLazily(() => _azureDevopsConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).Returns(Task.CompletedTask);

        A.CallTo(() => _appSettingsService.AzureDevOpsBaseUrl).Returns("https://dev.azure.com/MyOrg123ABC");
        A.CallTo(() => _appSettingsService.AzureDevOpsPersonalAccessToken).Returns("MY_TEST_PAT");

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
    
    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task RegisterServices_ShouldCopyExistingAppSettingsConfig_WhenNoCurrentCorrectConfig(int? version)
    {
        // arrange
        AzureDevopsConfigV1? persistedConfig = null;
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(version));
        RegisterExternals(_container);
        var sut = new AzureDevOpsPackage();
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        Fake.ClearRecordedCalls(_packageConfiguration);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).Invokes(call => persistedConfig = call.Arguments[0] as AzureDevopsConfigV1);

        // act
        // make sure everyting is resolved. This will trigger the copy of the config.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
        
        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<AzureDevopsConfigV1>._, 1)).MustHaveHappenedOnceExactly();
        await Verifier.Verify(persistedConfig).IgnoreParametersForVerified(nameof(version));
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

    private void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        container.RegisterSingleton(A.Dummy<IActionToRepositoryActionMapper>);
        container.RegisterSingleton(A.Dummy<ITranslationService>);
        container.RegisterInstance(_appSettingsService);
        container.RegisterSingleton(A.Dummy<ILogger>);
    }
}