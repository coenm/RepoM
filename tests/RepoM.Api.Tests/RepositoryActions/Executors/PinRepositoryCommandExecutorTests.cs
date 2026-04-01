namespace RepoM.Api.Tests.RepositoryActions.Executors;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.RepositoryActions.Executors;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories.Favorite;
using Xunit;

public class PinRepositoryCommandExecutorTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IFavoriteService _favoriteService = A.Fake<IFavoriteService>();
    private readonly PinRepositoryCommandExecutor _sut;

    public PinRepositoryCommandExecutorTests()
    {
        A.CallTo(() => _repository.SafePath).Returns("/repos/my-repo");
        _sut = new PinRepositoryCommandExecutor(_favoriteService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenFavoriteServiceIsNull()
    {
        // act
        Func<PinRepositoryCommandExecutor> act = () => new PinRepositoryCommandExecutor(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Execute_Pin_ShouldSetFavoriteTrue()
    {
        // act
        _sut.Execute(_repository, PinRepositoryCommand.Pin);

        // assert
        A.CallTo(() => _favoriteService.SetFavorite("/repos/my-repo", true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_UnPin_ShouldSetFavoriteFalse()
    {
        // act
        _sut.Execute(_repository, PinRepositoryCommand.UnPin);

        // assert
        A.CallTo(() => _favoriteService.SetFavorite("/repos/my-repo", false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_Toggle_ShouldSetFavoriteTrue_WhenCurrentlyNotFavorite()
    {
        // arrange
        A.CallTo(() => _favoriteService.IsFavorite("/repos/my-repo")).Returns(false);

        // act
        _sut.Execute(_repository, PinRepositoryCommand.Toggle);

        // assert
        A.CallTo(() => _favoriteService.SetFavorite("/repos/my-repo", true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_Toggle_ShouldSetFavoriteFalse_WhenCurrentlyFavorite()
    {
        // arrange
        A.CallTo(() => _favoriteService.IsFavorite("/repos/my-repo")).Returns(true);

        // act
        _sut.Execute(_repository, PinRepositoryCommand.Toggle);

        // assert
        A.CallTo(() => _favoriteService.SetFavorite("/repos/my-repo", false)).MustHaveHappenedOnceExactly();
    }
}
