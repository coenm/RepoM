namespace RepoM.Plugin.WindowsExplorerGitInfo.Tests;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Common;
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