namespace RepoM.Core.Repositories.Tests.Favorite;

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using FakeItEasy;
using RepoM.Core.Repositories.Favorite;
using Xunit;

public class FavoriteServiceTests
{
    private readonly IFavoriteStore _favoriteStore = A.Fake<IFavoriteStore>();
    private readonly FavoriteService _sut;

    public FavoriteServiceTests()
    {
        A.CallTo(() => _favoriteStore.Load()).Returns([]);
        _sut = new FavoriteService(_favoriteStore);
    }

    [Fact]
    public void IsFavorite_ShouldReturnFalse_WhenNotFavorite()
    {
        _sut.IsFavorite("/repos/test").Should().BeFalse();
    }

    [Fact]
    public void IsFavorite_ShouldReturnTrue_AfterFavoriting()
    {
        // Arrange
        _sut.SetFavorite("/repos/test", true);

        // Act & Assert
        _sut.IsFavorite("/repos/test").Should().BeTrue();
    }

    [Fact]
    public void IsFavorite_ShouldReturnFalse_AfterUnfavoriting()
    {
        // Arrange
        _sut.SetFavorite("/repos/test", true);
        _sut.SetFavorite("/repos/test", false);

        // Act & Assert
        _sut.IsFavorite("/repos/test").Should().BeFalse();
    }

    [Fact]
    public void IsFavorite_ShouldBeCaseInsensitive()
    {
        // Arrange
        _sut.SetFavorite("/repos/Test", true);

        // Act & Assert
        _sut.IsFavorite("/repos/test").Should().BeTrue();
        _sut.IsFavorite("/REPOS/TEST").Should().BeTrue();
    }

    [Fact]
    public void SetFavorite_ShouldBeIdempotent_WhenFavoritingTwice()
    {
        _sut.SetFavorite("/repos/test", true);
        _sut.SetFavorite("/repos/test", true);

        _sut.IsFavorite("/repos/test").Should().BeTrue();
    }

    [Fact]
    public void SetFavorite_ShouldNotThrow_WhenUnfavoritingNonExistentEntry()
    {
        var act = () => _sut.SetFavorite("/repos/test", false);
        act.Should().NotThrow();
    }

    [Fact]
    public void IsFavorite_ShouldThrow_WhenSafePathIsNull()
    {
        var act = () => _sut.IsFavorite(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsFavorite_ShouldThrow_WhenSafePathIsEmpty()
    {
        var act = () => _sut.IsFavorite(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetFavorite_ShouldThrow_WhenSafePathIsNull()
    {
        var act = () => _sut.SetFavorite(null!, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetFavorite_ShouldThrow_WhenSafePathIsEmpty()
    {
        var act = () => _sut.SetFavorite(string.Empty, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsFavorite_ShouldTrackMultipleRepositories()
    {
        // Arrange
        _sut.SetFavorite("/repos/a", true);
        _sut.SetFavorite("/repos/b", true);
        _sut.SetFavorite("/repos/c", false);

        // Assert
        _sut.IsFavorite("/repos/a").Should().BeTrue();
        _sut.IsFavorite("/repos/b").Should().BeTrue();
        _sut.IsFavorite("/repos/c").Should().BeFalse();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenFavoriteStoreIsNull()
    {
        // act
        Func<FavoriteService> act = () => new FavoriteService(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldLoadInitialFavoritesFromStore()
    {
        // arrange
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.Load()).Returns(["/repos/preloaded",]);

        // act
        using var sut = new FavoriteService(store);

        // assert
        sut.IsFavorite("/repos/preloaded").Should().BeTrue();
    }

    [Fact]
    public void GetAllFavorites_ShouldReturnEmpty_WhenNoneSet()
    {
        _sut.GetAllFavorites().Should().BeEmpty();
    }

    [Fact]
    public void GetAllFavorites_ShouldReturnAllFavorites()
    {
        // arrange
        _sut.SetFavorite("/repos/a", true);
        _sut.SetFavorite("/repos/b", true);

        // act
        var result = _sut.GetAllFavorites();

        // assert
        result.Should().HaveCount(2);
        result.Should().Contain("/repos/a");
        result.Should().Contain("/repos/b");
    }

    [Fact]
    public void GetAllFavorites_ShouldNotContainUnfavorited()
    {
        // arrange
        _sut.SetFavorite("/repos/a", true);
        _sut.SetFavorite("/repos/b", true);
        _sut.SetFavorite("/repos/a", false);

        // act
        var result = _sut.GetAllFavorites();

        // assert
        result.Should().HaveCount(1);
        result.Should().Contain("/repos/b");
    }

    [Fact]
    public void SetFavorite_ShouldRaiseFavoriteChanged_WhenFavorited()
    {
        // arrange
        var events = new List<(string path, bool favorite)>();
        _sut.FavoriteChanged += (path, fav) => events.Add((path, fav));

        // act
        _sut.SetFavorite("/repos/test", true);

        // assert
        events.Should().ContainSingle()
            .Which.Should().Be(("/repos/test", true));
    }

    [Fact]
    public void SetFavorite_ShouldRaiseFavoriteChanged_WhenUnfavorited()
    {
        // arrange
        _sut.SetFavorite("/repos/test", true);
        var events = new List<(string path, bool favorite)>();
        _sut.FavoriteChanged += (path, fav) => events.Add((path, fav));

        // act
        _sut.SetFavorite("/repos/test", false);

        // assert
        events.Should().ContainSingle()
            .Which.Should().Be(("/repos/test", false));
    }

    [Fact]
    public void SetFavorite_ShouldNotRaiseFavoriteChanged_WhenAlreadyFavorite()
    {
        // arrange
        _sut.SetFavorite("/repos/test", true);
        var events = new List<(string path, bool favorite)>();
        _sut.FavoriteChanged += (path, fav) => events.Add((path, fav));

        // act
        _sut.SetFavorite("/repos/test", true);

        // assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void SetFavorite_ShouldNotRaiseFavoriteChanged_WhenUnfavoritingNonExistent()
    {
        // arrange
        var events = new List<(string path, bool favorite)>();
        _sut.FavoriteChanged += (path, fav) => events.Add((path, fav));

        // act
        _sut.SetFavorite("/repos/test", false);

        // assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // act
        var act = () => _sut.Dispose();

        // assert
        act.Should().NotThrow();
    }
}
