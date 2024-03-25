namespace RepoM.Api.Tests.RepositoryActions.Executors;

using FakeItEasy;
using RepoM.Api.Git;
using RepoM.Api.RepositoryActions.Executors;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using Xunit;

public class GitRepositoryCommandExecutorTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IRepositoryWriter _repositoryWriter = A.Fake<IRepositoryWriter>();
    private readonly GitRepositoryCommandExecutor _sut;

    public GitRepositoryCommandExecutorTests()
    {
        _sut = new GitRepositoryCommandExecutor(_repositoryWriter);
    }

    [Fact]
    public void Execute_ShouldCallFetch_WhenCommandIsFetch()
    {
        // arrange

        // act
        _sut.Execute(_repository, GitRepositoryCommand.Fetch);

        // assert
        A.CallTo(() => _repositoryWriter.Fetch(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repositoryWriter).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldCallPull_WhenCommandIsPull()
    {
        // arrange

        // act
        _sut.Execute(_repository, GitRepositoryCommand.Pull);

        // assert
        A.CallTo(() => _repositoryWriter.Pull(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repositoryWriter).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldCallPush_WhenCommandIsPush()
    {
        // arrange

        // act
        _sut.Execute(_repository, GitRepositoryCommand.Push);

        // assert
        A.CallTo(() => _repositoryWriter.Push(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repositoryWriter).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldCallCheckout_WhenCommandIsCheckout()
    {
        // arrange

        // act
        _sut.Execute(_repository, GitRepositoryCommand.Checkout("branchXyz"));

        // assert
        A.CallTo(() => _repositoryWriter.Checkout(_repository, "branchXyz")).MustHaveHappenedOnceExactly();
        A.CallTo(_repositoryWriter).MustHaveHappenedOnceExactly();
    }
}