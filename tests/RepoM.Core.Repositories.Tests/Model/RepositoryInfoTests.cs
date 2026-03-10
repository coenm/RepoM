namespace RepoM.Core.Repositories.Tests.Model;

using System;
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

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameReference()
    {
        var sut = CreateDefault();
        sut.Equals(sut).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        var sut = CreateDefault();
        sut.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllObservablePropertiesAreEqual()
    {
        var a = CreateDefault();
        var b = CreateDefault();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldIgnoreLastSeen()
    {
        var a = CreateDefault();
        a.LastSeen = DateTimeOffset.UtcNow;

        var b = CreateDefault();
        b.LastSeen = DateTimeOffset.UtcNow.AddHours(-1);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldIgnoreLastUpdated()
    {
        var a = CreateDefault();
        a.LastUpdated = DateTimeOffset.UtcNow;

        var b = CreateDefault();
        b.LastUpdated = DateTimeOffset.UtcNow.AddDays(-1);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSafePathDiffers()
    {
        var a = CreateDefault();
        var b = new RepositoryInfo { Path = @"c:\repos\other", SafePath = "/repos/other", Name = "test", };

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCurrentBranchDiffers()
    {
        var a = CreateDefault();
        a.CurrentBranch = "main";

        var b = CreateDefault();
        b.CurrentBranch = "develop";

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCurrentBranchHasUpstreamDiffers()
    {
        var a = CreateDefault();
        a.CurrentBranchHasUpstream = true;

        var b = CreateDefault();
        b.CurrentBranchHasUpstream = false;

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCurrentBranchIsDetachedDiffers()
    {
        var a = CreateDefault();
        a.CurrentBranchIsDetached = true;

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCurrentBranchIsOnTagDiffers()
    {
        var a = CreateDefault();
        a.CurrentBranchIsOnTag = true;

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(RepositoryInfo.AheadBy))]
    [InlineData(nameof(RepositoryInfo.BehindBy))]
    [InlineData(nameof(RepositoryInfo.LocalUntracked))]
    [InlineData(nameof(RepositoryInfo.LocalModified))]
    [InlineData(nameof(RepositoryInfo.LocalMissing))]
    [InlineData(nameof(RepositoryInfo.LocalAdded))]
    [InlineData(nameof(RepositoryInfo.LocalStaged))]
    [InlineData(nameof(RepositoryInfo.LocalRemoved))]
    [InlineData(nameof(RepositoryInfo.LocalIgnored))]
    [InlineData(nameof(RepositoryInfo.StashCount))]
    public void Equals_ShouldReturnFalse_WhenNullableIntPropertyDiffers(string propertyName)
    {
        var a = CreateDefault();
        typeof(RepositoryInfo).GetProperty(propertyName)!.SetValue(a, 5);

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenWasFoundDiffers()
    {
        var a = CreateDefault();
        a.WasFound = false;

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenTagsDiffer()
    {
        var a = CreateDefault();
        a.Tags = ["v1.0",];

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenBranchesDiffer()
    {
        var a = CreateDefault();
        a.Branches = ["main", "develop",];

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenLocalBranchesDiffer()
    {
        var a = CreateDefault();
        a.LocalBranches = ["feature/x",];

        var b = CreateDefault();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_ShouldReturnTrue_WhenEqual()
    {
        var a = CreateDefault();
        object b = CreateDefault();

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_ShouldReturnFalse_WhenDifferentType()
    {
        var a = CreateDefault();
        a.Equals("not a repo").Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_ShouldReturnFalse_WhenObjectIsNull()
    {
        var a = CreateDefault();
        a.Equals((object?)null).Should().BeFalse();
    }

    // --- GetHashCode tests ---

    [Fact]
    public void GetHashCode_ShouldBeEqual_WhenObjectsAreEqual()
    {
        var a = CreateDefault();
        a.CurrentBranch = "main";
        a.AheadBy = 1;
        a.BehindBy = 2;
        a.StashCount = 3;

        var b = CreateDefault();
        b.CurrentBranch = "main";
        b.AheadBy = 1;
        b.BehindBy = 2;
        b.StashCount = 3;

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldDiffer_WhenSafePathDiffers()
    {
        var a = CreateDefault();
        var b = new RepositoryInfo { Path = @"c:\repos\other", SafePath = "/repos/other", Name = "other", };

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldDiffer_WhenCurrentBranchDiffers()
    {
        var a = CreateDefault();
        a.CurrentBranch = "main";

        var b = CreateDefault();
        b.CurrentBranch = "develop";

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        var sut = CreateDefault();
        sut.CurrentBranch = "main";

        var hash1 = sut.GetHashCode();
        var hash2 = sut.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_ShouldNotThrow_WhenAllNullableFieldsAreNull()
    {
        var sut = CreateDefault();

        Action act = () => sut.GetHashCode();

        act.Should().NotThrow();
    }
}
