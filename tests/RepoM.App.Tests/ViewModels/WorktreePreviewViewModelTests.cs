namespace RepoM.App.Tests.ViewModels;

using System.Linq;
using AwesomeAssertions;
using RepoM.App.ViewModels;
using Xunit;

public class WorktreePreviewViewModelTests
{
    [Fact]
    public void CycleDisplayMode_ShouldCycleThreeStatesAndHideOnlyOwnWorktrees()
    {
        var sut = new WorktreePreviewViewModel();
        WorktreePreviewFamily family = sut.Families[0];

        family.Primary.CycleDisplayMode();

        family.Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);
        family.Primary.AreDetailsVisible.Should().BeFalse();
        sut.VisibleEntries.Should().Contain(family.Primary);
        sut.VisibleEntries.Should().NotContain(entry => family.Worktrees.Contains(entry));
        sut.VisibleEntries.Should().Contain(sut.Families[1].Worktrees);

        family.Primary.CycleDisplayMode();

        family.Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Extended);
        family.Primary.AreDetailsVisible.Should().BeTrue();
        sut.VisibleEntries.Should().NotContain(entry => family.Worktrees.Contains(entry));

        family.Primary.CycleDisplayMode();

        family.Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.ExtendedWithWorktrees);
        sut.VisibleEntries.Take(3).Should().Equal(new[] { family.Primary }.Concat(family.Worktrees));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CycleDisplayMode_WithoutChildren_ShouldToggleDetailsIndependently(bool isWorktree)
    {
        var sut = new WorktreePreviewViewModel();
        WorktreePreviewEntry entry = isWorktree ? sut.Families[0].Worktrees[0] : sut.Families[2].Primary;
        entry.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Extended);

        entry.CycleDisplayMode();

        entry.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);
        entry.AreDetailsVisible.Should().BeFalse();
        sut.VisibleEntries.Should().Contain(entry);
        sut.Families[0].Primary.AreWorktreesVisible.Should().BeTrue();
        sut.Families[0].Worktrees[1].AreDetailsVisible.Should().BeTrue();

        entry.CycleDisplayMode();

        entry.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Extended);
        entry.AreWorktreesVisible.Should().BeFalse();
    }

    [Theory]
    [InlineData(" LOGIN ")]
    [InlineData("feature/login")]
    [InlineData(@"C:\dev\RepoM.worktrees\feature-login")]
    public void Filter_ShouldRevealMatchingWorktreeAndParentDespiteCollapsedState(string filter)
    {
        var sut = new WorktreePreviewViewModel();
        WorktreePreviewFamily family = sut.Families[0];
        family.Primary.DisplayMode = WorktreePreviewDisplayMode.Minimal;
        family.Worktrees[0].DisplayMode = WorktreePreviewDisplayMode.Minimal;

        sut.Filter = filter;

        sut.VisibleEntries.Should().Equal(family.Primary, family.Worktrees[0]);
        family.Primary.IsContextOnly.Should().BeTrue();
        family.Primary.AreWorktreesVisible.Should().BeTrue();
        family.Primary.CanChangeDisplayMode.Should().BeFalse();
        family.Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);
        family.Primary.AreDetailsVisible.Should().BeTrue();
        family.Worktrees[0].AreDetailsVisible.Should().BeTrue();
        family.Worktrees[0].CanChangeDisplayMode.Should().BeFalse();
        family.Primary.CycleDisplayMode();
        family.Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);

        sut.Filter = string.Empty;

        family.Primary.IsContextOnly.Should().BeFalse();
        family.Primary.AreWorktreesVisible.Should().BeFalse();
        family.Primary.CanChangeDisplayMode.Should().BeTrue();
        family.Primary.AreDetailsVisible.Should().BeFalse();
        family.Worktrees[0].AreDetailsVisible.Should().BeFalse();
        sut.VisibleEntries.Should().NotContain(entry => family.Worktrees.Contains(entry));
    }

    [Fact]
    public void Filter_ShouldKeepMatchingPrimaryWithoutUnmatchedChildren()
    {
        var sut = new WorktreePreviewViewModel { Filter = "develop" };

        sut.VisibleEntries.Should().Equal(sut.Families[1].Primary);
        sut.Families[1].Primary.IsContextOnly.Should().BeFalse();
        sut.Families[1].Primary.AreWorktreesVisible.Should().BeFalse();
    }

    [Fact]
    public void FilterByWorktreeTag_ShouldKeepPrimaryAsContextAndHideSiblings()
    {
        var sut = new WorktreePreviewViewModel();
        sut.Families[0].Primary.DisplayMode = WorktreePreviewDisplayMode.Extended;

        sut.Filter = " AUTHENTICATION ";

        sut.VisibleEntries.Should().Equal(sut.Families[0].Primary, sut.Families[0].Worktrees[0]);
        sut.Families[0].Primary.IsContextOnly.Should().BeTrue();
        sut.Families[0].Primary.AreWorktreesVisible.Should().BeTrue();

        sut.Filter = string.Empty;

        sut.Families[0].Primary.DisplayedMode.Should().Be(WorktreePreviewDisplayMode.Extended);
        sut.Families[0].Primary.AreWorktreesVisible.Should().BeFalse();
    }

    [Fact]
    public void ClearingFilter_ShouldRestoreExpandedFamiliesAndPreviewFavorites()
    {
        var sut = new WorktreePreviewViewModel();
        WorktreePreviewEntry worktree = sut.Families[0].Worktrees[0];
        worktree.IsFavorite = true;

        sut.Filter = "does-not-exist";

        sut.IsEmpty.Should().BeTrue();
        sut.VisibleEntries.Should().BeEmpty();

        sut.Filter = " ";

        sut.IsEmpty.Should().BeFalse();
        sut.VisibleEntries.Should().Contain(worktree);
        worktree.IsFavorite.Should().BeTrue();
        sut.Families[0].Primary.AreWorktreesVisible.Should().BeTrue();
    }
}
