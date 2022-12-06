namespace RepoM.Plugin.Statistics.Tests;

using System;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using SimpleInjector;
using Xunit;

public class StatisticsPackageTest
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new StatisticsPackage();

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
        var sut = new StatisticsPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IClock>);
        container.RegisterSingleton(A.Dummy<IAppDataPathProvider>);
        container.RegisterSingleton(A.Dummy<ILogger<StatisticsModule>>);
    }
}