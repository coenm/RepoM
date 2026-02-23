namespace RepoM.Core.Repositories.Tests.Store;

using System;
using System.Reactive.Linq;
using AwesomeAssertions;
using DynamicData;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;
using Xunit;

public class RepositoryStoreTests : IDisposable
{
    private readonly RepositoryStore _sut = new();

    [Fact]
    public void Count_ShouldBeZero_WhenEmpty()
    {
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void AddOrUpdate_ShouldAddRepository()
    {
        // Arrange
        var repo = CreateRepo("/repos/test", "test");

        // Act
        _sut.AddOrUpdate(repo);

        // Assert
        _sut.Count.Should().Be(1);
        _sut.Lookup(repo.SafePath).HasValue.Should().BeTrue();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingRepository_WhenSameKey()
    {
        // Arrange
        var repo1 = CreateRepo("/repos/test", "test");
        repo1.CurrentBranch = "main";

        var repo2 = CreateRepo("/repos/test", "test");
        repo2.CurrentBranch = "develop";

        // Act
        _sut.AddOrUpdate(repo1);
        _sut.AddOrUpdate(repo2);

        // Assert
        _sut.Count.Should().Be(1);
        _sut.Lookup("/repos/test").Value.CurrentBranch.Should().Be("develop");
    }

    [Fact]
    public void Remove_ShouldRemoveRepository()
    {
        // Arrange
        var repo = CreateRepo("/repos/test", "test");
        _sut.AddOrUpdate(repo);

        // Act
        _sut.Remove(repo.SafePath);

        // Assert
        _sut.Count.Should().Be(0);
        _sut.Lookup(repo.SafePath).HasValue.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllRepositories()
    {
        // Arrange
        _sut.AddOrUpdate(CreateRepo("/repos/a", "a"));
        _sut.AddOrUpdate(CreateRepo("/repos/b", "b"));
        _sut.AddOrUpdate(CreateRepo("/repos/c", "c"));

        // Act
        _sut.Clear();

        // Assert
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void Connect_ShouldEmitChangeSet_WhenRepositoryAdded()
    {
        // Arrange
        var repo = CreateRepo("/repos/test", "test");
        ChangeSet<RepositoryInfo, string>? receivedChangeSet = null;

        _sut.Connect()
            .Subscribe(cs => receivedChangeSet = new ChangeSet<RepositoryInfo, string>(cs));

        // Act
        _sut.AddOrUpdate(repo);

        // Assert
        receivedChangeSet.Should().NotBeNull();
        receivedChangeSet!.Adds.Should().Be(1);
    }

    [Fact]
    public void Connect_WithPredicate_ShouldOnlyEmitMatchingItems()
    {
        // Arrange
        var repo1 = CreateRepo("/repos/a", "a");
        repo1.LocalModified = 1; // HasLocalChanges will be true

        var repo2 = CreateRepo("/repos/b", "b");
        // HasLocalChanges will be false (all counters default to null)

        var receivedCount = 0;

        _sut.Connect(r => r.HasLocalChanges)
            .Subscribe(cs =>
            {
                foreach (var change in cs)
                {
                    if (change.Reason == ChangeReason.Add)
                    {
                        receivedCount++;
                    }
                }
            });

        // Act
        _sut.AddOrUpdate(repo1);
        _sut.AddOrUpdate(repo2);

        // Assert
        receivedCount.Should().Be(1);
    }

    [Fact]
    public void Items_ShouldReturnAllRepositories()
    {
        // Arrange
        _sut.AddOrUpdate(CreateRepo("/repos/a", "a"));
        _sut.AddOrUpdate(CreateRepo("/repos/b", "b"));

        // Act & Assert
        _sut.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddOrUpdate_ShouldThrow_WhenNull()
    {
        var act = () => _sut.AddOrUpdate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Remove_ShouldThrow_WhenNullOrEmpty()
    {
        var act = () => _sut.Remove(null!);
        act.Should().Throw<ArgumentException>();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    private static RepositoryInfo CreateRepo(string safePath, string name)
    {
        return new RepositoryInfo
        {
            Path = safePath.Replace('/', '\\'),
            SafePath = safePath,
            Name = name,
        };
    }
}
