namespace RepoM.Core.Repositories.Tests.Pinning;

using System;
using AwesomeAssertions;
using RepoM.Core.Repositories.Pinning;
using Xunit;

public class PinningServiceTests
{
    private readonly PinningService _sut = new();

    [Fact]
    public void IsPinned_ShouldReturnFalse_WhenNotPinned()
    {
        _sut.IsPinned("/repos/test").Should().BeFalse();
    }

    [Fact]
    public void IsPinned_ShouldReturnTrue_AfterPinning()
    {
        // Arrange
        _sut.SetPinned("/repos/test", true);

        // Act & Assert
        _sut.IsPinned("/repos/test").Should().BeTrue();
    }

    [Fact]
    public void IsPinned_ShouldReturnFalse_AfterUnpinning()
    {
        // Arrange
        _sut.SetPinned("/repos/test", true);
        _sut.SetPinned("/repos/test", false);

        // Act & Assert
        _sut.IsPinned("/repos/test").Should().BeFalse();
    }

    [Fact]
    public void IsPinned_ShouldBeCaseInsensitive()
    {
        // Arrange
        _sut.SetPinned("/repos/Test", true);

        // Act & Assert
        _sut.IsPinned("/repos/test").Should().BeTrue();
        _sut.IsPinned("/REPOS/TEST").Should().BeTrue();
    }

    [Fact]
    public void SetPinned_ShouldBeIdempotent_WhenPinningTwice()
    {
        _sut.SetPinned("/repos/test", true);
        _sut.SetPinned("/repos/test", true);

        _sut.IsPinned("/repos/test").Should().BeTrue();
    }

    [Fact]
    public void SetPinned_ShouldNotThrow_WhenUnpinningNonExistentEntry()
    {
        var act = () => _sut.SetPinned("/repos/test", false);
        act.Should().NotThrow();
    }

    [Fact]
    public void IsPinned_ShouldThrow_WhenSafePathIsNull()
    {
        var act = () => _sut.IsPinned(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsPinned_ShouldThrow_WhenSafePathIsEmpty()
    {
        var act = () => _sut.IsPinned(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPinned_ShouldThrow_WhenSafePathIsNull()
    {
        var act = () => _sut.SetPinned(null!, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPinned_ShouldThrow_WhenSafePathIsEmpty()
    {
        var act = () => _sut.SetPinned(string.Empty, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsPinned_ShouldTrackMultipleRepositories()
    {
        // Arrange
        _sut.SetPinned("/repos/a", true);
        _sut.SetPinned("/repos/b", true);
        _sut.SetPinned("/repos/c", false);

        // Assert
        _sut.IsPinned("/repos/a").Should().BeTrue();
        _sut.IsPinned("/repos/b").Should().BeTrue();
        _sut.IsPinned("/repos/c").Should().BeFalse();
    }
}
