namespace RepoM.Plugin.Clipboard.Tests;

using FakeItEasy;
using RepoM.Core.Plugin;
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

    // This test is commented out because it has no external dependencies.
    // [Fact]
    // public void RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    // {
    //     // arrange
    //     var container = new Container();
    //     var sut = new ClipboardPackage();
    //
    //     // act
    //     sut.RegisterServices(container);
    //
    //     // assert
    //     Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    // }

    private static void RegisterExternals(Container container)
    {
        // intentionally left blank.
    }
}

file static class PackageExtensions
{
    // tmp for fixing tests.
    public static void RegisterServices(this IPackage self, Container container)
    {
        self.RegisterServicesAsync(container, null!).GetAwaiter().GetResult();
    }
}