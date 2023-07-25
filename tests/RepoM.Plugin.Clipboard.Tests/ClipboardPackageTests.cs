namespace RepoM.Plugin.Clipboard.Tests;

using System;
using FakeItEasy;
using RepoM.Api.Common;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Expressions;
using SimpleInjector;
using Xunit;

public class ClipboardPackageTests
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new ClipboardPackage();

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
        var sut = new ClipboardPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        container.RegisterSingleton(A.Dummy<ITranslationService>);
    }
}

file static class ExtensionIPackageWithConfiguration
{
    // tmp for fixing tests.
    public static void RegisterServices(this IPackageWithConfiguration self, Container container)
    {
        self.RegisterServicesAsync(container, null!).GetAwaiter().GetResult();
    }
}