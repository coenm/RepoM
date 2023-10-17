namespace RepoM.Plugin.WebBrowser.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.WebBrowser;
using RepoM.Plugin.WebBrowser.PersistentConfiguration;
using SimpleInjector;
using Xunit;

public class WebBrowserPackageTest
{
    private readonly Container _container;
    private readonly IPackageConfiguration _packageConfiguration;

    public WebBrowserPackageTest()
    {
        _packageConfiguration = A.Fake<IPackageConfiguration>();
        _container = new Container();

        var webBrowserConfigV1 = new WebBrowserConfigV1
            {
                Browsers = new Dictionary<string, string>
                {
                    { "Edge", "msedge.exe" },
                },
                Profiles = new Dictionary<string, ProfileConfig>
                {
                    {
                        "incognito",
                        new ProfileConfig { BrowserName = "Edge", CommandLineArguments = "--incognito", }
                    },
                },
            };
        A.CallTo(() => _packageConfiguration.GetConfigurationVersionAsync()).Returns(Task.FromResult(1 as int?));
        A.CallTo(() => _packageConfiguration.LoadConfigurationAsync<WebBrowserConfigV1>()).ReturnsLazily(() => webBrowserConfigV1);
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<WebBrowserConfigV1>._, 1)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new WebBrowserPackage();

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
        var sut = new WebBrowserPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        A.CallTo(() => _packageConfiguration.PersistConfigurationAsync(A<WebBrowserConfigV1>._, 1)).MustHaveHappenedOnceExactly();

        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public async Task RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new WebBrowserPackage();

        // act
        await sut.RegisterServicesAsync(_container, _packageConfiguration);

        // assert
        Assert.Throws<InvalidOperationException>(() => _container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        container.RegisterSingleton(A.Dummy<IActionToRepositoryActionMapper>);
    }
}