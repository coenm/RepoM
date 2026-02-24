namespace RepoM.Core.Repositories.Tests.Adapters;

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
using Xunit;

public class RepositoryInfoAdapterTests
{
    private readonly RepositoryInfo _info;
    private readonly RepositoryInfoAdapter _sut;

    public RepositoryInfoAdapterTests()
    {
        _info = new RepositoryInfo
        {
            Path = @"c:\repos\test",
            SafePath = "/repos/test",
            Name = "test",
            WindowsPath = @"c:\repos\test",
            LinuxPath = "/repos/test",
            Location = @"c:\repos",
            IsBare = false,
            Remotes = { new Remote("origin", "https://github.com/test/test.git"), },
        };
        _info.CurrentBranch = "main";
        _info.Branches = ["main", "develop",];
        _info.LocalBranches = ["main",];
        _info.Tags = ["v1.0",];

        _sut = new RepositoryInfoAdapter(_info);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInfoIsNull()
    {
        var act = () => new RepositoryInfoAdapter(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RepositoryInfo_ShouldReturnWrappedInstance()
    {
        _sut.RepositoryInfo.Should().BeSameAs(_info);
    }

    [Fact]
    public void Name_ShouldDelegateToInfo()
    {
        _sut.Name.Should().Be("test");
    }

    [Fact]
    public void Path_ShouldDelegateToInfo()
    {
        _sut.Path.Should().Be(@"c:\repos\test");
    }

    [Fact]
    public void SafePath_ShouldDelegateToInfo()
    {
        _sut.SafePath.Should().Be("/repos/test");
    }

    [Fact]
    public void WindowsPath_ShouldDelegateToInfo()
    {
        _sut.WindowsPath.Should().Be(@"c:\repos\test");
    }

    [Fact]
    public void LinuxPath_ShouldDelegateToInfo()
    {
        _sut.LinuxPath.Should().Be("/repos/test");
    }

    [Fact]
    public void Location_ShouldDelegateToInfo()
    {
        _sut.Location.Should().Be(@"c:\repos");
    }

    [Fact]
    public void IsBare_ShouldDelegateToInfo()
    {
        _sut.IsBare.Should().BeFalse();
    }

    [Fact]
    public void CurrentBranch_ShouldDelegateToInfo()
    {
        _sut.CurrentBranch.Should().Be("main");
    }

    [Fact]
    public void Branches_ShouldDelegateToInfo()
    {
        _sut.Branches.Should().BeEquivalentTo(["main", "develop",]);
    }

    [Fact]
    public void LocalBranches_ShouldDelegateToInfo()
    {
        _sut.LocalBranches.Should().BeEquivalentTo(["main",]);
    }

    [Fact]
    public void Tags_ShouldDelegateToInfo()
    {
        _sut.Tags.Should().BeEquivalentTo(["v1.0",]);
    }

    [Fact]
    public void Remotes_ShouldDelegateToInfo()
    {
        _sut.Remotes.Should().HaveCount(1);
        _sut.Remotes[0].Key.Should().Be("origin");
    }

    [Fact]
    public void HasUnpushedChanges_ShouldDelegateToInfo()
    {
        _info.AheadBy = 1;
        _sut.HasUnpushedChanges.Should().BeTrue();
    }

    [Fact]
    public void HasLocalChanges_ShouldDelegateToInfo()
    {
        _info.LocalModified = 1;
        _sut.HasLocalChanges.Should().BeTrue();
    }

    [Fact]
    public void IsBehind_ShouldDelegateToInfo()
    {
        _info.BehindBy = 2;
        _sut.IsBehind.Should().BeTrue();
    }

    [Fact]
    public void ReadAllBranches_ShouldReturnEmpty_WhenReaderIsNull()
    {
        _sut.ReadAllBranches().Should().BeEmpty();
    }

    [Fact]
    public void ReadAllBranches_ShouldInvokeReader_WhenSet()
    {
        // Arrange
        var info = new RepositoryInfo
        {
            Path = @"c:\repos\test",
            SafePath = "/repos/test",
            Name = "test",
            AllBranchesReader = () => ["main", "feature/x",],
        };
        var sut = new RepositoryInfoAdapter(info);

        // Act
        var result = sut.ReadAllBranches();

        // Assert
        result.Should().BeEquivalentTo(["main", "feature/x",]);
    }
}
