namespace RepoM.Core.Repositories.Tests.Model;

using AwesomeAssertions;
using RepoM.Core.Repositories.Model;
using Xunit;

public class RepositoryInfoTests
{
    private static RepositoryInfo CreateDefault()
    {
        return new RepositoryInfo
        {
            Path = @"c:\repos\test",
            SafePath = "/repos/test",
            Name = "test",
        };
    }

    [Fact]
    public void GetStatusCode_ShouldReturnDashSeparatedValues()
    {
        // Arrange
        var sut = CreateDefault();
        sut.CurrentBranch = "main";
        sut.AheadBy = 1;
        sut.BehindBy = 2;
        sut.LocalUntracked = 3;
        sut.LocalModified = 4;
        sut.LocalMissing = 5;
        sut.LocalAdded = 6;
        sut.LocalStaged = 7;
        sut.LocalRemoved = 8;
        sut.LocalIgnored = 9;
        sut.StashCount = 10;

        // Act
        var result = sut.GetStatusCode();

        // Assert
        result.Should().Be("main-1-2-3-4-5-6-7-8-9-10");
    }

    [Fact]
    public void GetStatusCode_ShouldTreatNullsAsZero()
    {
        // Arrange
        var sut = CreateDefault();
        sut.CurrentBranch = "develop";

        // Act
        var result = sut.GetStatusCode();

        // Assert
        result.Should().Be("develop-0-0-0-0-0-0-0-0-0-0");
    }

    [Fact]
    public void HasUnpushedChanges_ShouldBeFalse_WhenAllCountersAreNull()
    {
        var sut = CreateDefault();
        sut.HasUnpushedChanges.Should().BeFalse();
    }

    [Fact]
    public void HasUnpushedChanges_ShouldBeFalse_WhenAllCountersAreZero()
    {
        var sut = CreateDefault();
        sut.AheadBy = 0;
        sut.LocalUntracked = 0;
        sut.LocalModified = 0;
        sut.LocalMissing = 0;
        sut.LocalAdded = 0;
        sut.LocalStaged = 0;
        sut.LocalRemoved = 0;
        sut.StashCount = 0;

        sut.HasUnpushedChanges.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(RepositoryInfo.AheadBy))]
    [InlineData(nameof(RepositoryInfo.LocalUntracked))]
    [InlineData(nameof(RepositoryInfo.LocalModified))]
    [InlineData(nameof(RepositoryInfo.LocalMissing))]
    [InlineData(nameof(RepositoryInfo.LocalAdded))]
    [InlineData(nameof(RepositoryInfo.LocalStaged))]
    [InlineData(nameof(RepositoryInfo.LocalRemoved))]
    [InlineData(nameof(RepositoryInfo.StashCount))]
    public void HasUnpushedChanges_ShouldBeTrue_WhenAnyCounterIsPositive(string propertyName)
    {
        var sut = CreateDefault();
        typeof(RepositoryInfo).GetProperty(propertyName)!.SetValue(sut, 1);
        sut.HasUnpushedChanges.Should().BeTrue();
    }

    [Fact]
    public void HasLocalChanges_ShouldBeFalse_WhenAllLocalCountersAreNull()
    {
        var sut = CreateDefault();
        sut.HasLocalChanges.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(RepositoryInfo.LocalUntracked))]
    [InlineData(nameof(RepositoryInfo.LocalModified))]
    [InlineData(nameof(RepositoryInfo.LocalMissing))]
    [InlineData(nameof(RepositoryInfo.LocalAdded))]
    [InlineData(nameof(RepositoryInfo.LocalStaged))]
    [InlineData(nameof(RepositoryInfo.LocalRemoved))]
    public void HasLocalChanges_ShouldBeTrue_WhenAnyLocalCounterIsPositive(string propertyName)
    {
        var sut = CreateDefault();
        typeof(RepositoryInfo).GetProperty(propertyName)!.SetValue(sut, 1);
        sut.HasLocalChanges.Should().BeTrue();
    }

    [Fact]
    public void HasLocalChanges_ShouldBeFalse_WhenOnlyAheadByOrStashCountIsPositive()
    {
        var sut = CreateDefault();
        sut.AheadBy = 5;
        sut.StashCount = 3;
        sut.HasLocalChanges.Should().BeFalse();
    }

    [Fact]
    public void IsBehind_ShouldBeFalse_WhenBehindByIsNull()
    {
        var sut = CreateDefault();
        sut.IsBehind.Should().BeFalse();
    }

    [Fact]
    public void IsBehind_ShouldBeFalse_WhenBehindByIsZero()
    {
        var sut = CreateDefault();
        sut.BehindBy = 0;
        sut.IsBehind.Should().BeFalse();
    }

    [Fact]
    public void IsBehind_ShouldBeTrue_WhenBehindByIsPositive()
    {
        var sut = CreateDefault();
        sut.BehindBy = 3;
        sut.IsBehind.Should().BeTrue();
    }

    [Fact]
    public void WasFound_ShouldDefaultToTrue()
    {
        var sut = CreateDefault();
        sut.WasFound.Should().BeTrue();
    }

    [Fact]
    public void Branches_ShouldDefaultToEmptyArray()
    {
        var sut = CreateDefault();
        sut.Branches.Should().BeEmpty();
    }

    [Fact]
    public void Tags_ShouldDefaultToEmptyArray()
    {
        var sut = CreateDefault();
        sut.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Remotes_ShouldDefaultToEmptyList()
    {
        var sut = CreateDefault();
        sut.Remotes.Should().BeEmpty();
    }
}
