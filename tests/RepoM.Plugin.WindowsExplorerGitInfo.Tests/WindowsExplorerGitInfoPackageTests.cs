namespace RepoM.Plugin.WindowsExplorerGitInfo.Tests;

using System;
using FakeItEasy;
using RepoM.Api.Git;
using RepoM.Core.Plugin;
using SimpleInjector;
using Xunit;

public class WindowsExplorerGitInfoPackageTests
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new WindowsExplorerGitInfoPackage();

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
        var sut = new WindowsExplorerGitInfoPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IRepositoryInformationAggregator>);
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