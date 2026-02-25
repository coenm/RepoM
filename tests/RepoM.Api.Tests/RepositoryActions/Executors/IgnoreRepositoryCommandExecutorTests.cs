namespace RepoM.Api.Tests.RepositoryActions.Executors;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Git;
using RepoM.Api.RepositoryActions.Executors;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using Xunit;

public class IgnoreRepositoryCommandExecutorTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore = A.Fake<IRepositoryIgnoreStore>();
    private readonly IgnoreRepositoryCommandExecutor _sut;

    public IgnoreRepositoryCommandExecutorTests()
    {
        _sut = new IgnoreRepositoryCommandExecutor(_repositoryIgnoreStore);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<IgnoreRepositoryCommandExecutor> act = () => new IgnoreRepositoryCommandExecutor(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Execute_ShouldCallIgnoreByPath_WithRepositoryPath()
    {
        // arrange
        A.CallTo(() => _repository.Path).Returns("C:/repos/my-repo");

        // act
        _sut.Execute(_repository, IgnoreRepositoryCommand.Instance);

        // assert
        A.CallTo(() => _repositoryIgnoreStore.IgnoreByPath("C:/repos/my-repo")).MustHaveHappenedOnceExactly();
        A.CallTo(_repositoryIgnoreStore).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldNotThrow_WhenIgnoreByPathThrows()
    {
        // arrange
        A.CallTo(() => _repositoryIgnoreStore.IgnoreByPath(A<string>._)).Throws(new InvalidOperationException("test"));

        // act
        var act = () => _sut.Execute(_repository, IgnoreRepositoryCommand.Instance);

        // assert
        act.Should().NotThrow();
    }
}
