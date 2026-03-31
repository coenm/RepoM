namespace RepoM.Core.Repositories.Tests.Favorite;

using System;
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
}
