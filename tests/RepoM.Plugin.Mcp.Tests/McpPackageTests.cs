namespace RepoM.Plugin.Mcp.Tests;

using System;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Core.Repositories.Store;
using SimpleInjector;
using Xunit;

public class McpPackageTests
{
    [Fact]
    public void Name_ShouldReturn_McpPackage()
    {
        // arrange
        var sut = new McpPackage();

        // act & assert
        Assert.Equal("McpPackage", sut.Name);
    }

    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new McpPackage();

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
        var sut = new McpPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Fake<IRepositoryStore>);
        container.RegisterSingleton(A.Fake<ILogger>);
    }
}

file static class PackageExtensions
{
    public static void RegisterServices(this IPackage self, Container container)
    {
        var packageConfiguration = A.Fake<IPackageConfiguration>();
        self.RegisterServicesAsync(container, packageConfiguration).GetAwaiter().GetResult();
    }
}
