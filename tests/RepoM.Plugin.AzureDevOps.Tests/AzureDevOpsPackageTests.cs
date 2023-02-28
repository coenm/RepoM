namespace RepoM.Plugin.AzureDevOps.Tests;

using System;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin.Expressions;
using SimpleInjector;
using Xunit;

public class AzureDevOpsPackageTests
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new AzureDevOpsPackage();

        // act
        sut.RegisterServices(container);

        // assert
        // implicit, Verify throws when container is not valid.
        container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public void RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var container = new Container();
        var sut = new AzureDevOpsPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        container.RegisterSingleton(A.Dummy<IActionToRepositoryActionMapper>);
        container.RegisterSingleton(A.Dummy<ITranslationService>);
        container.RegisterSingleton(A.Dummy<IAppSettingsService>);
        container.RegisterSingleton(A.Dummy<ILogger>);
    }
}