namespace RepoM.Api.Tests.Git;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AwesomeAssertions;
using FakeItEasy;
using RepoM.Api.Git;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Pinning;
using Xunit;

public class RepositoryViewModelTests
{
    private readonly RepositoryBuilder _builder = new();
    private readonly IPinningService _pinningService = A.Fake<IPinningService>();

    private RepositoryViewModel CreateSut(RepositoryInfo? info = null)
    {
        return new RepositoryViewModel(info ?? _builder.BuildFullFeatured(), _pinningService);
    }

    public class Ctor : RepositoryViewModelTests
    {
        [Fact]
        public void Throws_When_Info_Is_Null()
        {
            Action act = () => _ = new RepositoryViewModel(null!, _pinningService);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Throws_When_PinningService_Is_Null()
        {
            Action act = () => _ = new RepositoryViewModel(_builder.BuildFullFeatured(), null!);
            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class Properties : RepositoryViewModelTests
    {
        [Fact]
        public void Name_Returns_Name_From_Info()
        {
            RepositoryInfo info = _builder.WithName("MyRepo").WithCurrentBranch("main").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Name.Should().Be("MyRepo");
        }

        [Fact]
        public void Path_Returns_Path_From_Info()
        {
            RepositoryInfo info = _builder.WithPath(@"C:\Dev\Repo\").WithName("Repo").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Path.Should().Be(@"C:\Dev\Repo\");
        }

        [Fact]
        public void CurrentBranch_Returns_CurrentBranch_From_Info()
        {
            RepositoryInfo info = _builder.WithCurrentBranch("develop").WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.CurrentBranch.Should().Be("develop");
        }

        [Fact]
        public void AheadBy_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithAheadBy(5).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.AheadBy.Should().Be("5");
        }

        [Fact]
        public void AheadBy_Returns_Empty_When_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.AheadBy.Should().BeEmpty();
        }

        [Fact]
        public void BehindBy_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithBehindBy(3).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.BehindBy.Should().Be("3");
        }

        [Fact]
        public void BehindBy_Returns_Empty_When_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.BehindBy.Should().BeEmpty();
        }

        [Fact]
        public void Branches_Returns_Branches_From_Info()
        {
            RepositoryInfo info = _builder.WithBranches("master", "dev").WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Branches.Should().BeEquivalentTo("master", "dev");
        }

        [Fact]
        public void Branches_Returns_Empty_Array_When_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Branches.Should().BeEmpty();
        }

        [Fact]
        public void LocalUntracked_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalUntracked(7).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalUntracked.Should().Be("7");
        }

        [Fact]
        public void LocalUntracked_Returns_Empty_When_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalUntracked.Should().BeEmpty();
        }

        [Fact]
        public void LocalModified_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalModified(2).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalModified.Should().Be("2");
        }

        [Fact]
        public void LocalMissing_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalMissing(4).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalMissing.Should().Be("4");
        }

        [Fact]
        public void LocalAdded_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalAdded(1).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalAdded.Should().Be("1");
        }

        [Fact]
        public void LocalStaged_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalStaged(6).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalStaged.Should().Be("6");
        }

        [Fact]
        public void LocalRemoved_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalRemoved(3).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalRemoved.Should().Be("3");
        }

        [Fact]
        public void StashCount_Returns_Value_As_String()
        {
            RepositoryInfo info = _builder.FullFeatured().WithStashCount(2).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.StashCount.Should().Be("2");
        }

        [Fact]
        public void StashCount_Returns_Empty_When_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.StashCount.Should().BeEmpty();
        }

        [Fact]
        public void WasFound_Returns_Value_From_Info()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(info);

            sut.WasFound.Should().BeTrue();
        }

        [Fact]
        public void HasUnpushedChanges_Returns_Value_From_Info()
        {
            RepositoryInfo info = _builder.FullFeatured().WithAheadBy(1).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.HasUnpushedChanges.Should().BeTrue();
        }

        [Fact]
        public void IsNotBare_Returns_True_When_Not_Bare()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(info);

            sut.IsNotBare.Should().BeTrue();
        }

        [Fact]
        public void Tags_Constructed_From_Info_Tags()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            info.Tags = new[] { "v1.0", "v2.0", };
            RepositoryViewModel sut = CreateSut(info);

            sut.Tags.Should().HaveCount(2);
            sut.Tags[0].Tag.Should().Be("v1.0");
            sut.Tags[1].Tag.Should().Be("v2.0");
        }

        [Fact]
        public void Tags_Empty_When_Info_Has_No_Tags()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Repository_Returns_Adapter()
        {
            RepositoryViewModel sut = CreateSut();

            sut.Repository.Should().NotBeNull();
            sut.Repository.Name.Should().Be("Repo");
        }

        [Fact]
        public void RepositoryInfo_Returns_Current_Info()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(info);

            sut.RepositoryInfo.Should().BeSameAs(info);
        }
    }

    public class IsPinnedProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_True_When_Pinned()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            A.CallTo(() => _pinningService.IsPinned(info.SafePath)).Returns(true);
            RepositoryViewModel sut = CreateSut(info);

            sut.IsPinned.Should().BeTrue();
        }

        [Fact]
        public void Returns_False_When_Not_Pinned()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            A.CallTo(() => _pinningService.IsPinned(info.SafePath)).Returns(false);
            RepositoryViewModel sut = CreateSut(info);

            sut.IsPinned.Should().BeFalse();
        }
    }

    public class StatusProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_Compressed_Status()
        {
            RepositoryInfo info = _builder.FullFeatured().Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.Status.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Caches_Status_On_Subsequent_Calls()
        {
            RepositoryInfo info = _builder.FullFeatured().Build();
            RepositoryViewModel sut = CreateSut(info);

            var first = sut.Status;
            var second = sut.Status;

            first.Should().BeSameAs(second);
        }
    }

    public class BranchWithStatusProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_Branch_With_Compressed_Status()
        {
            RepositoryInfo info = _builder.FullFeatured().Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.BranchWithStatus.Should().StartWith("master");
        }
    }

    public class IsSynchronizingProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Name_Includes_Sync_Appendix_When_Synchronizing()
        {
            RepositoryViewModel sut = CreateSut();
            sut.IsSynchronizing = true;

            sut.Name.Should().Contain("\u2191\u2193");
        }

        [Fact]
        public void Name_Excludes_Sync_Appendix_When_Not_Synchronizing()
        {
            RepositoryViewModel sut = CreateSut();
            sut.IsSynchronizing = false;

            sut.Name.Should().NotContain("\u2191\u2193");
        }

        [Fact]
        public void Setting_IsSynchronizing_Raises_PropertyChanged_For_Name()
        {
            RepositoryViewModel sut = CreateSut();
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            sut.IsSynchronizing = true;

            raised.Should().Contain(nameof(RepositoryViewModel.Name));
        }
    }

    public class EqualsMethod : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_True_For_Same_Path()
        {
            RepositoryInfo info1 = _builder.WithPath(@"C:\Repo\").WithName("R").Build();
            var sut1 = new RepositoryViewModel(info1, _pinningService);

            var builder2 = new RepositoryBuilder();
            RepositoryInfo info2 = builder2.WithPath(@"C:\Repo\").WithName("R").Build();
            var sut2 = new RepositoryViewModel(info2, _pinningService);

            sut1.Equals(sut2).Should().BeTrue();
        }

        [Fact]
        public void Returns_True_For_Same_Path_Different_Case()
        {
            RepositoryInfo info1 = _builder.WithPath(@"C:\Repo\").WithName("R").Build();
            var sut1 = new RepositoryViewModel(info1, _pinningService);

            var builder2 = new RepositoryBuilder();
            RepositoryInfo info2 = builder2.WithPath(@"C:\REPO\").WithName("R").Build();
            var sut2 = new RepositoryViewModel(info2, _pinningService);

            sut1.Equals(sut2).Should().BeTrue();
        }

        [Fact]
        public void Returns_False_For_Different_Path()
        {
            RepositoryInfo info1 = _builder.WithPath(@"C:\Repo1\").WithName("R").Build();
            var sut1 = new RepositoryViewModel(info1, _pinningService);

            var builder2 = new RepositoryBuilder();
            RepositoryInfo info2 = builder2.WithPath(@"C:\Repo2\").WithName("R").Build();
            var sut2 = new RepositoryViewModel(info2, _pinningService);

            sut1.Equals(sut2).Should().BeFalse();
        }

        [Fact]
        public void Returns_False_For_Non_RepositoryViewModel()
        {
            RepositoryViewModel sut = CreateSut();
            sut.Equals("not a vm").Should().BeFalse();
        }

        [Fact]
        public void Returns_True_For_Same_Reference()
        {
            RepositoryViewModel sut = CreateSut();
            sut.Equals(sut).Should().BeTrue();
        }
    }

    public class GetHashCodeMethod : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_Consistent_HashCode()
        {
            RepositoryViewModel sut = CreateSut();
            sut.GetHashCode().Should().Be(sut.GetHashCode());
        }

        [Fact]
        public void Equal_ViewModels_Have_Same_HashCode()
        {
            RepositoryInfo info1 = _builder.WithPath(@"C:\Repo\").WithName("R").Build();
            var sut1 = new RepositoryViewModel(info1, _pinningService);

            var builder2 = new RepositoryBuilder();
            RepositoryInfo info2 = builder2.WithPath(@"C:\Repo\").WithName("R").Build();
            var sut2 = new RepositoryViewModel(info2, _pinningService);

            sut1.GetHashCode().Should().Be(sut2.GetHashCode());
        }
    }

    public class UpdateMethod : RepositoryViewModelTests
    {
        [Fact]
        public void Throws_When_NewInfo_Is_Null()
        {
            RepositoryViewModel sut = CreateSut();
            Action act = () => sut.Update(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Updates_RepositoryInfo()
        {
            RepositoryInfo original = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(original);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithName("UpdatedRepo").Build();
            sut.Update(updated);

            sut.RepositoryInfo.Should().BeSameAs(updated);
        }

        [Fact]
        public void Raises_PropertyChanged_For_CurrentBranch_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithCurrentBranch("main").Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithCurrentBranch("develop").Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.CurrentBranch));
            raised.Should().Contain(nameof(RepositoryViewModel.Name));
        }

        [Fact]
        public void Does_Not_Raise_PropertyChanged_For_CurrentBranch_When_Unchanged()
        {
            RepositoryInfo original = _builder.FullFeatured().WithCurrentBranch("main").Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithCurrentBranch("main").Build();
            sut.Update(updated);

            raised.Should().NotContain(nameof(RepositoryViewModel.CurrentBranch));
        }

        [Fact]
        public void Raises_PropertyChanged_For_AheadBy_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithAheadBy(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithAheadBy(5).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.AheadBy));
        }

        [Fact]
        public void Raises_PropertyChanged_For_BehindBy_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithBehindBy(2).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithBehindBy(10).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.BehindBy));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalUntracked_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalUntracked(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalUntracked(5).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalUntracked));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalModified_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalModified(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalModified(8).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalModified));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalMissing_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalMissing(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalMissing(3).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalMissing));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalAdded_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalAdded(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalAdded(4).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalAdded));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalStaged_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalStaged(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalStaged(7).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalStaged));
        }

        [Fact]
        public void Raises_PropertyChanged_For_LocalRemoved_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalRemoved(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalRemoved(9).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.LocalRemoved));
        }

        [Fact]
        public void Raises_PropertyChanged_For_StashCount_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithStashCount(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithStashCount(5).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.StashCount));
        }

        [Fact]
        public void Raises_PropertyChanged_For_HasUnpushedChanges_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithAheadBy(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.WithName("Repo").WithPath(@"C:\Develop\Repo\").WithCurrentBranch("master").Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.HasUnpushedChanges));
        }

        [Fact]
        public void Raises_PropertyChanged_For_WasFound_When_Changed()
        {
            RepositoryInfo original = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().Build();
            updated.WasFound = false;
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.WasFound));
        }

        [Fact]
        public void Raises_PropertyChanged_For_Branches_When_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithBranches("master").Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithBranches("master", "feature").Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.Branches));
        }

        [Fact]
        public void Does_Not_Raise_PropertyChanged_For_Branches_When_Unchanged()
        {
            RepositoryInfo original = _builder.FullFeatured().WithBranches("master", "dev").Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithBranches("master", "dev").Build();
            sut.Update(updated);

            raised.Should().NotContain(nameof(RepositoryViewModel.Branches));
        }

        [Fact]
        public void Raises_PropertyChanged_For_Status_When_StatusCode_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalModified(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalModified(99).Build();
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.Status));
            raised.Should().Contain(nameof(RepositoryViewModel.BranchWithStatus));
        }

        [Fact]
        public void Raises_PropertyChanged_For_Tags_When_Changed()
        {
            RepositoryInfo original = _builder.BuildFullFeatured();
            original.Tags = new[] { "v1.0", };
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.BuildFullFeatured();
            updated.Tags = new[] { "v1.0", "v2.0", };
            sut.Update(updated);

            raised.Should().Contain(nameof(RepositoryViewModel.Tags));
            sut.Tags.Should().HaveCount(2);
        }

        [Fact]
        public void Does_Not_Raise_PropertyChanged_For_Tags_When_Unchanged()
        {
            RepositoryInfo original = _builder.BuildFullFeatured();
            original.Tags = new[] { "v1.0", };
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.BuildFullFeatured();
            updated.Tags = new[] { "v1.0", };
            sut.Update(updated);

            raised.Should().NotContain(nameof(RepositoryViewModel.Tags));
        }

        [Fact]
        public void Invalidates_Status_Cache()
        {
            RepositoryInfo original = _builder.FullFeatured().WithLocalModified(1).Build();
            RepositoryViewModel sut = CreateSut(original);
            var statusBefore = sut.Status;

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured().WithLocalModified(99).Build();
            sut.Update(updated);

            sut.Status.Should().NotBeSameAs(statusBefore);
        }

        [Fact]
        public void No_PropertyChanged_When_Nothing_Changed()
        {
            RepositoryInfo original = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(original);
            var raised = new List<string>();
            sut.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.BuildFullFeatured();
            sut.Update(updated);

            raised.Should().BeEmpty();
        }

        [Fact]
        public void Update_Does_Not_Throw_When_No_Subscriber_And_Properties_Changed()
        {
            RepositoryInfo original = _builder.FullFeatured()
                .WithCurrentBranch("main")
                .WithBranches("main")
                .Build();
            original.Tags = ["v1.0",];
            RepositoryViewModel sut = CreateSut(original);

            // No PropertyChanged subscriber attached
            var newBuilder = new RepositoryBuilder();
            RepositoryInfo updated = newBuilder.FullFeatured()
                .WithCurrentBranch("develop")
                .WithBranches("main", "develop")
                .Build();
            updated.Tags = ["v1.0", "v2.0",];

            Action act = () => sut.Update(updated);

            act.Should().NotThrow();
        }
    }

    public class LocationProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_Location_From_Info()
        {
            RepositoryInfo info = _builder.BuildFullFeatured();
            RepositoryViewModel sut = CreateSut(info);

            sut.Location.Should().Be(info.Location);
        }
    }

    public class BranchesProperty : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_Empty_Array_When_Branches_Is_Null()
        {
            RepositoryInfo info = _builder.WithName("R").Build();
            info.Branches = null!;
            RepositoryViewModel sut = CreateSut(info);

            sut.Branches.Should().BeEmpty();
        }
    }

    public class NullableIntToStringCoverage : RepositoryViewModelTests
    {
        [Fact]
        public void Returns_String_For_Value_Above_SmallIntCache()
        {
            RepositoryInfo info = _builder.FullFeatured().WithLocalModified(150).Build();
            RepositoryViewModel sut = CreateSut(info);

            sut.LocalModified.Should().Be("150");
        }
    }
}
