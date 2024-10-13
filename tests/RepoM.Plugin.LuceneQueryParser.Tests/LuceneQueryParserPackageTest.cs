namespace RepoM.Plugin.LuceneQueryParser.Tests;

using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using SimpleInjector;
using Xunit;

public class LuceneQueryParserPackageTest
{
    private readonly Container _container;

    public LuceneQueryParserPackageTest()
    {
        _container = new Container();
    }

    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new LuceneQueryParserPackage();

        // act
        sut.RegisterServices(_container);

        // assert
        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public void RegisterServices_ShouldRegisterLuceneQueryParserAsCollection()
    {
        // arrange
        RegisterExternals(_container);
        var sut = new LuceneQueryParserPackage();
        sut.RegisterServices(_container);

        // act
        INamedQueryParser[] instances = _container.GetAllInstances<INamedQueryParser>().ToArray();

        // assert
        _ = instances.Should().ContainSingle().And.AllBeOfType<LuceneQueryParser>();
    }

    [Fact]
    public void RegisterServices_ShouldFail_WhenExternalDependenciesAreNotRegistered()
    {
        // arrange
        var sut = new LuceneQueryParserPackage();

        // act
        sut.RegisterServices(_container);
        Action act = () => _container.Verify(VerificationOption.VerifyAndDiagnose);

        // assert
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<ILogger>);
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