namespace Tests.UI;

using System;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RepoM.Api.Git;
using Tests.Helper;

public class RepositoryViewTests
{
    private Repository _repo = null!;
    private readonly RepositoryBuilder _repositoryBuilder = new RepositoryBuilder().FullFeatured();
    private RepositoryViewModel _viewModel = null!;
    private StatusCharacterMap _statusCharacterMap = null!;

    [SetUp]
    public void Setup()
    {
        _repo = new RepositoryBuilder().BuildFullFeatured();
        _viewModel = new RepositoryViewModel(_repo, A.Dummy<IRepositoryMonitor>());
        _statusCharacterMap = new StatusCharacterMap();
    }

    public class CtorMethod : RepositoryViewTests
    {
        [Test]
        public void Throws_If_Null_Is_Passed_As_Argument()
        {
            Action act = () => _ = new RepositoryViewModel(null!, A.Dummy<IRepositoryMonitor>());
            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class AheadByProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.AheadBy.Should().Be(_repo.AheadBy.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.AheadBy = null;
            _viewModel.AheadBy.Should().BeEmpty();
        }
    }

    public class BehindByProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.BehindBy.Should().Be(_repo.BehindBy.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.BehindBy = null;
            _viewModel.BehindBy.Should().BeEmpty();
        }
    }

    public class BranchesProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.Branches.Should().ContainInOrder("master", "feature-one", "feature-two");
        }

        [Test]
        public void Returns_An_Empty_Array_For_Null()
        {
            _repo.Branches = null!;
            _viewModel.Branches.Length.Should().Be(0);
        }
    }

    public class CurrentBranchProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value()
        {
            _viewModel.CurrentBranch.Should().Be(_repo.CurrentBranch);
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.CurrentBranch = null!;
            _viewModel.CurrentBranch.Should().BeEmpty();
        }
    }

    public class LocalAddedProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalAdded.Should().Be(_repo.LocalAdded.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalAdded = null;
            _viewModel.LocalAdded.Should().BeEmpty();
        }
    }

    public class LocalIgnoredProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalIgnored.Should().Be(_repo.LocalIgnored.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalIgnored = null;
            _viewModel.LocalIgnored.Should().BeEmpty();
        }
    }

    public class StashCountProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.StashCount.Should().Be(_repo.StashCount.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.StashCount = null;
            _viewModel.StashCount.Should().BeEmpty();
        }
    }

    public class LocalMissingProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalMissing.Should().Be(_repo.LocalMissing.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalMissing = null;
            _viewModel.LocalMissing.Should().BeEmpty();
        }
    }

    public class LocalModifiedProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalModified.Should().Be(_repo.LocalModified.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalModified = null;
            _viewModel.LocalModified.Should().BeEmpty();
        }
    }

    public class LocalRemovedProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalRemoved.Should().Be(_repo.LocalRemoved.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalRemoved = null;
            _viewModel.LocalRemoved.Should().BeEmpty();
        }
    }

    public class LocalStagedProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalStaged.Should().Be(_repo.LocalStaged.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalStaged = null;
            _viewModel.LocalStaged.Should().BeEmpty();
        }
    }

    public class LocalUntrackedProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value_As_String()
        {
            _viewModel.LocalUntracked.Should().Be(_repo.LocalUntracked.ToString());
        }

        [Test]
        public void Returns_An_Empty_String_For_Null()
        {
            _repo.LocalUntracked = null;
            _viewModel.LocalUntracked.Should().BeEmpty();
        }
    }

    public class NameProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value()
        {
            _viewModel.Name.Should().Be(_repo.Name);
        }
    }

    public class PathProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value()
        {
            _viewModel.Path.Should().Be(_repo.Path);
        }
    }

    public class StatusProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Compressed_String_From_The_StatusCompressor_Helper_Class()
        {
            var expected = new StatusCompressor(_statusCharacterMap).Compress(_repo);
            _viewModel.Status.Should().Be(expected);
        }
    }

    public class WasFoundProperty : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value()
        {
            _viewModel.WasFound.Should().Be(_repo.WasFound);
        }

        [Test]
        public void Returns_False_If_Path_Is_Empty()
        {
            _repositoryBuilder.WithPath(string.Empty);
            Repository repo = _repositoryBuilder.Build();

            var viewModel = new RepositoryViewModel(repo, A.Dummy<IRepositoryMonitor>());

            viewModel.WasFound.Should().BeFalse();
        }
    }

    public class GetHashCodeMethod : RepositoryViewTests
    {
        [Test]
        public void Returns_The_Repository_Value()
        {
            _viewModel.GetHashCode().Should().Be(_repo.GetHashCode());
        }
    }
}