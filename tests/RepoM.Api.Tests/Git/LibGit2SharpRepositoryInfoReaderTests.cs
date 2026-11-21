namespace RepoM.Api.Tests.Git;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.Git.ProcessExecution;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin.Repository;
using Xunit;
using GitCommandException = RepoM.Api.Git.ProcessExecution.GitCommandException;

public sealed class LibGit2SharpRepositoryInfoReaderTests : IDisposable
{
    private const string InitialHeaders = "# branch.oid (initial)\0# branch.head cli-branch\0";
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("RepoM-reader-tests-");
    private readonly IRepositoryTagsFactory _tags = A.Fake<IRepositoryTagsFactory>();
    private readonly ILogger _logger = A.Fake<ILogger>();
    private readonly LibGit2SharpRepositoryInfoReader _sut;

    public LibGit2SharpRepositoryInfoReaderTests()
    {
        LibGit2Sharp.Repository.Init(_directory.FullName);
        _sut = new LibGit2SharpRepositoryInfoReader(_tags, new ProcessExecutingGitCommander(_logger), _logger);
        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).Returns(Task.FromResult<IEnumerable<string>>(["test-tag"]));
    }

    [Fact]
    public async Task ReadAsync_ShouldMeasureStagesWithoutChangingRepositoryData()
    {
        await File.WriteAllTextAsync(Path.Combine(_directory.FullName, "untracked.txt"), "test");

        var result = await _sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.LocalUntracked.Should().Be(1);
        result.Tags.Should().Equal("test-tag");
        result.CurrentBranchHasUpstream.Should().BeFalse();
        result.AheadBy.Should().BeNull();
        result.BehindBy.Should().BeNull();
        result.CurrentBranchIsDetached.Should().BeFalse();
        var states = Fake.GetCalls(_logger)
            .Where(call => call.Method.Name == nameof(ILogger.Log))
            .Select(call => call.Arguments[2])
            .OfType<IEnumerable<KeyValuePair<string, object?>>>()
            .Select(state => state.ToDictionary(pair => pair.Key, pair => pair.Value))
            .Where(state => state.ContainsKey("ReadId"))
            .ToArray();
        states.Select(state => state["Stage"]).Should().Equal("discover", "open", "status", "branches", "metadata", "repom-tags", "total");
        states.Select(state => state["ReadId"]).Distinct().Should().ContainSingle();
        states.Should().OnlyContain(state => Equals(state["Outcome"], "completed"));
        states.Select(state => state["ElapsedMs"]).OfType<double>().Should().OnlyContain(elapsed => elapsed >= 0);
        states.Single(state => Equals(state["Stage"], "status"))["Backend"].Should().Be("git-cli");
    }

    [Fact]
    public async Task ReadAsync_ShouldStopAfterCliStatus_WhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        A.CallTo(_logger).Where(call => call.Method.Name == nameof(ILogger.Log)
                && call.Arguments[2]!.ToString()!.Contains("stage=status;", StringComparison.Ordinal))
            .Invokes(() => cts.Cancel());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ReadAsync(_directory.FullName, cts.Token));

        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).MustNotHaveHappened();
        LogMessages().Should().Contain(message => message.Contains("stage=total;", StringComparison.Ordinal)
            && message.Contains("outcome=cancelled", StringComparison.Ordinal));
        LogMessages().Should().NotContain(message => message.Contains("stage=head;", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReadAsync_ShouldNotPublishResult_WhenCancelledDuringTags()
    {
        using var cts = new CancellationTokenSource();
        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).Invokes(() => cts.Cancel())
            .Returns(Task.FromResult<IEnumerable<string>>([]));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ReadAsync(_directory.FullName, cts.Token));

        LogMessages().Should().Contain(message => message.Contains("stage=repom-tags;", StringComparison.Ordinal)
            && message.Contains("outcome=cancelled", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReadAsync_ShouldLogAndPropagateTagFailure()
    {
        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).ThrowsAsync(new InvalidOperationException("tag failure"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ReadAsync(_directory.FullName));

        LogMessages().Should().Contain(message => message.Contains("stage=repom-tags;", StringComparison.Ordinal)
            && message.Contains("outcome=failed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReadAsync_ShouldMatchPreviousStatusCounts()
    {
        using var repo = new LibGit2Sharp.Repository(_directory.FullName);
        repo.Config.Set("core.autocrlf", false);
        repo.Config.Set("status.renames", true);
        foreach (var name in new[] { "modified.txt", "staged.txt", "missing.txt", "removed.txt", "renamed.txt" })
        {
            File.WriteAllText(Path.Combine(_directory.FullName, name), "original\n");
        }

        File.WriteAllText(Path.Combine(_directory.FullName, ".gitignore"), "ignored/\n");
        LibGit2Sharp.Commands.Stage(repo, "*");
        var signature = new LibGit2Sharp.Signature("RepoM test", "test@example.invalid", DateTimeOffset.UtcNow);
        repo.Commit("Initial test files", signature, signature, new LibGit2Sharp.CommitOptions());
        File.WriteAllText(Path.Combine(_directory.FullName, "modified.txt"), "changed\n");
        File.WriteAllText(Path.Combine(_directory.FullName, "staged.txt"), "staged change\n");
        LibGit2Sharp.Commands.Stage(repo, "staged.txt");
        File.WriteAllText(Path.Combine(_directory.FullName, "staged.txt"), "changed after staging\n");
        File.Delete(Path.Combine(_directory.FullName, "missing.txt"));
        LibGit2Sharp.Commands.Remove(repo, "removed.txt");
        File.Move(Path.Combine(_directory.FullName, "renamed.txt"), Path.Combine(_directory.FullName, "new name.txt"));
        LibGit2Sharp.Commands.Stage(repo, new[] { "renamed.txt", "new name.txt" });
        var nested = Directory.CreateDirectory(Path.Combine(_directory.FullName, "untracked folder"));
        File.WriteAllText(Path.Combine(nested.FullName, "one.txt"), "one");
        File.WriteAllText(Path.Combine(nested.FullName, "two.txt"), "two");
        var ignored = Directory.CreateDirectory(Path.Combine(_directory.FullName, "ignored"));
        File.WriteAllText(Path.Combine(ignored.FullName, "ignored.txt"), "ignored");
        var expected = repo.RetrieveStatus(new LibGit2Sharp.StatusOptions
        {
            IncludeIgnored = false,
            DetectRenamesInIndex = false,
            DetectRenamesInWorkDir = false,
        });

        var result = await _sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.LocalUntracked.Should().Be(expected.Untracked.Count());
        result.LocalModified.Should().Be(expected.Modified.Count());
        result.LocalAdded.Should().Be(expected.Added.Count());
        result.LocalStaged.Should().Be(expected.Staged.Count());
        result.LocalMissing.Should().Be(expected.Missing.Count());
        result.LocalRemoved.Should().Be(expected.Removed.Count());
        result.CurrentBranch.Should().Be(repo.Head.FriendlyName);
    }

    [Fact]
    public async Task ReadAsync_ShouldParseNulDelimitedNamesAndSeparateIndexAndWorktreeChanges()
    {
        var commander = A.Fake<IGitCommander>();
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._))
            .Returns(InitialHeaders
                + "1 MM N... 100644 100644 100644 a b both.txt\0"
                + "1 AM N... 000000 100644 100644 a b added and modified.txt\0"
                + "1 .D N... 100644 100644 000000 a b missing.txt\0"
                + "1 D. N... 100644 000000 000000 a b removed.txt\0"
                + "? with\nnewline.txt\0? with spaces.txt\0"
                + "u UU N... 100644 100644 100644 100644 a b c conflict.txt\0"
                + "u AA N... 000000 100644 100644 100644 a b c added conflict.txt\0"
                + "1 .T N... 100644 100644 120000 a b type-change.txt\0");
        var sut = new LibGit2SharpRepositoryInfoReader(_tags, commander, _logger);

        var result = await sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.LocalUntracked.Should().Be(2);
        result.LocalModified.Should().Be(2);
        result.LocalAdded.Should().Be(1);
        result.LocalStaged.Should().Be(1);
        result.LocalMissing.Should().Be(1);
        result.LocalRemoved.Should().Be(1);
        result.CurrentBranch.Should().Be("cli-branch");
    }

    [Fact]
    public async Task ReadAsync_ShouldUseCliTrackingSnapshotRatherThanNativeHead()
    {
        var commander = A.Fake<IGitCommander>();
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._))
            .Returns("# branch.oid 0123456789012345678901234567890123456789\0"
                + "# branch.head cli-branch\0# branch.upstream origin/cli-branch\0# branch.ab +0 -5\0");
        var sut = new LibGit2SharpRepositoryInfoReader(_tags, commander, _logger);

        var result = await sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.CurrentBranch.Should().Be("cli-branch");
        result.CurrentBranchHasUpstream.Should().BeTrue();
        result.AheadBy.Should().Be(0);
        result.BehindBy.Should().Be(5);
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("")]
    [InlineData(InitialHeaders + "? missing-terminator.txt")]
    [InlineData(InitialHeaders + "1 M. N... 100644 100644 100644 a b \0")]
    [InlineData("unexpected output\0")]
    [InlineData(InitialHeaders + "2 R. N... 100644 100644 100644 a b R100 renamed.txt\0old.txt\0")]
    [InlineData(InitialHeaders + "# branch.ab +1 -2\0")]
    [InlineData("# branch.head main\0")]
    [InlineData("# branch.oid (initial)\0# branch.head (detached)\0")]
    [InlineData(InitialHeaders + "# branch.head duplicate\0")]
    public async Task ReadAsync_ShouldRejectMalformedStatusInsteadOfReportingClean(string output)
    {
        var commander = A.Fake<IGitCommander>();
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._)).Returns(output);
        var sut = new LibGit2SharpRepositoryInfoReader(_tags, commander, _logger);

        await Assert.ThrowsAsync<GitCommandException>(() => sut.ReadAsync(_directory.FullName));

        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).MustNotHaveHappened();
        LogMessages().Should().Contain(message => message.Contains("stage=status;", StringComparison.Ordinal)
            && message.Contains("outcome=failed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReadAsync_ShouldCancelPendingCliStatus()
    {
        var commander = A.Fake<IGitCommander>();
        using var cts = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._))
            .ReturnsLazily(async call =>
            {
                entered.SetResult();
                await Task.Delay(Timeout.Infinite, call.GetArgument<CancellationToken>(1));
                return string.Empty;
            });
        var sut = new LibGit2SharpRepositoryInfoReader(_tags, commander, _logger);

        var read = sut.ReadAsync(_directory.FullName, cts.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read.WaitAsync(TimeSpan.FromSeconds(5)));

        A.CallTo(() => _tags.GetTagsAsync(A<IRepository>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CliCommand_ShouldReportNonzeroExitCode()
    {
        var commander = new ProcessExecutingGitCommander(_logger);

        var exception = await Assert.ThrowsAsync<GitCommandException>(() =>
            commander.CommandAsync(_directory.FullName, CancellationToken.None, "rev-parse", "--verify", "missing-branch"));

        exception.Message.Should().Contain("error code");
    }

    [Fact]
    public async Task CliCommand_ShouldTerminateRunningProcessWhenCancelled()
    {
        using (var repo = new LibGit2Sharp.Repository(_directory.FullName))
        {
            repo.Config.Set("alias.repom-test-wait", "!sleep 30");
        }

        var commander = new ProcessExecutingGitCommander(_logger);
        using var cts = new CancellationTokenSource();
        var command = commander.CommandAsync(_directory.FullName, cts.Token, "repom-test-wait");
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => command.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task ReadAsync_ShouldSkipCliStatusForBareRepository()
    {
        var barePath = Path.Combine(_directory.FullName, "bare.git");
        LibGit2Sharp.Repository.Init(barePath, isBare: true);
        var commander = A.Fake<IGitCommander>();
        var sut = new LibGit2SharpRepositoryInfoReader(_tags, commander, _logger);

        var result = await sut.ReadAsync(barePath);

        result.Should().NotBeNull();
        result!.IsBare.Should().BeTrue();
        result.LocalUntracked.Should().BeNull();
        result.LocalStaged.Should().BeNull();
        A.CallTo(() => commander.CommandAsync(A<string>._, A<CancellationToken>._, A<string[]>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadAsync_ShouldReadTrackingFromCliIncludingMissingUpstream(bool deleteUpstream)
    {
        using var repo = new LibGit2Sharp.Repository(_directory.FullName);
        var signature = new LibGit2Sharp.Signature("RepoM test", "test@example.invalid", DateTimeOffset.UtcNow);
        var options = new LibGit2Sharp.CommitOptions { AllowEmptyCommit = true };
        var initial = repo.Commit("initial", signature, signature, options);
        var branchName = repo.Head.FriendlyName;
        repo.Branches.Add("upstream", initial);
        repo.Config.Set($"branch.{branchName}.remote", ".");
        repo.Config.Set($"branch.{branchName}.merge", "refs/heads/upstream");
        LibGit2Sharp.Commands.Checkout(repo, "upstream");
        repo.Commit("upstream change", signature, signature, options);
        LibGit2Sharp.Commands.Checkout(repo, branchName);
        repo.Commit("local change", signature, signature, options);
        if (deleteUpstream)
        {
            repo.Branches.Remove("upstream");
        }

        var result = await _sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.CurrentBranch.Should().Be(branchName);
        result.CurrentBranchIsDetached.Should().BeFalse();
        result.CurrentBranchHasUpstream.Should().BeTrue();
        result.AheadBy.Should().Be(deleteUpstream ? null : 1);
        result.BehindBy.Should().Be(deleteUpstream ? null : 1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadAsync_ShouldPreserveDetachedHeadDisplay(bool tagged)
    {
        using var repo = new LibGit2Sharp.Repository(_directory.FullName);
        var signature = new LibGit2Sharp.Signature("RepoM test", "test@example.invalid", DateTimeOffset.UtcNow);
        var commit = repo.Commit("initial", signature, signature, new LibGit2Sharp.CommitOptions { AllowEmptyCommit = true });
        if (tagged)
        {
            repo.Tags.Add("release-test", commit.Id.Sha);
        }

        LibGit2Sharp.Commands.Checkout(repo, commit);

        var result = await _sut.ReadAsync(_directory.FullName);

        result.Should().NotBeNull();
        result!.CurrentBranch.Should().Be(tagged ? "release-test" : commit.Id.Sha);
        result.CurrentBranchIsDetached.Should().BeTrue();
        result.CurrentBranchIsOnTag.Should().Be(tagged);
        result.CurrentBranchHasUpstream.Should().BeFalse();
        result.AheadBy.Should().BeNull();
        result.BehindBy.Should().BeNull();
    }

    private string[] LogMessages() => Fake.GetCalls(_logger)
        .Where(call => call.Method.Name == nameof(ILogger.Log))
        .Select(call => call.Arguments[2]!.ToString()!).ToArray();

    public void Dispose()
    {
        foreach (var file in _directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes &= ~FileAttributes.ReadOnly;
        }

        _directory.Delete(true);
    }
}
