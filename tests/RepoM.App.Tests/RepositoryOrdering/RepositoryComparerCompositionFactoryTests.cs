namespace RepoM.App.Tests.RepositoryOrdering;

using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.App.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;
using Xunit;

public class RepositoryComparerCompositionFactoryTests
{
    private readonly Container _container = new();
    private readonly ILogger _logger = A.Fake<ILogger>();

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<RepositoryComparerCompositionFactory> act1 = () => new RepositoryComparerCompositionFactory(new Container(), null!);
        Func<RepositoryComparerCompositionFactory> act2 = () => new RepositoryComparerCompositionFactory(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenContainerCannotCreateInstance()
    {
        // arrange
        var sut = new RepositoryComparerCompositionFactory(_container, _logger);

        // act
        Func<IRepositoryComparer> act = () => sut.Create(new DummyConfiguration());

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenCreatedFactoryThrows()
    {
        // arrange
        _container.RegisterSingleton<IRepositoryComparerFactory<DummyConfiguration>, FailingRepositoryComparerFactoryDummyConfiguration>();
        var sut = new RepositoryComparerCompositionFactory(_container, _logger);

        // act
        Func<IRepositoryComparer> act = () => sut.Create(new DummyConfiguration());

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_ShouldReturnInstance_WhenFactoryCreatesInstance()
    {
        // arrange
        IRepositoryComparer comparer = A.Fake<IRepositoryComparer>();
        _container.RegisterSingleton<IRepositoryComparerFactory<DummyConfiguration>>(() => new RepositoryComparerFactoryDummyConfiguration(comparer));
        var sut = new RepositoryComparerCompositionFactory(_container, _logger);

        // act
        IRepositoryComparer result = sut.Create(new DummyConfiguration());

        // assert
        result.Should().Be(comparer);
    }
}

public class FailingRepositoryComparerFactoryDummyConfiguration : IRepositoryComparerFactory<DummyConfiguration>
{
    public IRepositoryComparer Create(DummyConfiguration configuration)
    {
        throw new NotImplementedException("Thrown by test");
    }
}

public class RepositoryComparerFactoryDummyConfiguration : IRepositoryComparerFactory<DummyConfiguration>
{
    private readonly IRepositoryComparer _instance;

    public RepositoryComparerFactoryDummyConfiguration(IRepositoryComparer instance)
    {
        _instance = instance;
    }
    public IRepositoryComparer Create(DummyConfiguration configuration)
    {
        return _instance;
    }
}

public class DummyConfiguration : IRepositoriesComparerConfiguration
{
    public string Type
    {
        get => "DUMMY";
        set => _ = value;
    }
}