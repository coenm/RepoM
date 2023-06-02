namespace RepoM.Plugin.SonarCloud.Tests;

using System;
using FakeItEasy;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.SonarCloud;
using SimpleInjector;
using Xunit;

public class SonarCloudPackageTest
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        RegisterExternals(container);
        var sut = new SonarCloudPackage();

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
        var sut = new SonarCloudPackage();

        // act
        sut.RegisterServices(container);

        // assert
        Assert.Throws<InvalidOperationException>(() => container.Verify(VerificationOption.VerifyAndDiagnose));
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<IAppSettingsService>);
        container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>);
        container.RegisterSingleton(A.Dummy<ITranslationService>);
    }
}