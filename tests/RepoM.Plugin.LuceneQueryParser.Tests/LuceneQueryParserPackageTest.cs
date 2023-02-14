namespace RepoM.Plugin.LuceneQueryParser.Tests;

using System.Linq;
using FluentAssertions;
using RepoM.Core.Plugin.RepositoryFiltering;
using SimpleInjector;
using Xunit;

public class LuceneQueryParserPackageTest
{
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        var container = new Container();
        var sut = new LuceneQueryParserPackage();

        // act
        sut.RegisterServices(container);

        // assert
        // implicit, Verify throws when container is not valid.
        container.Verify(VerificationOption.VerifyAndDiagnose);
    }

    [Fact]
    public void RegisterServices_ShouldRegisterLuceneQueryParserAsCollection()
    {
        // arrange
        var container = new Container();
        var sut = new LuceneQueryParserPackage();
        sut.RegisterServices(container);

        // act
        INamedQueryParser[] instances = container.GetAllInstances<INamedQueryParser>().ToArray();

        // assert
        _ = instances.Should().HaveCount(1).And.AllBeOfType<LuceneQueryParser>();
    }
}