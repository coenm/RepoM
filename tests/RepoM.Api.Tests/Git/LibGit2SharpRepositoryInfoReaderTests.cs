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
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin.Repository;
using Xunit;

public sealed class LibGit2SharpRepositoryInfoReaderTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("RepoM-reader-tests-");
    private readonly IRepositoryTagsFactory _tags = A.Fake<IRepositoryTagsFactory>();
    private readonly ILogger _logger = A.Fake<ILogger>();
    private readonly LibGit2SharpRepositoryInfoReader _sut;

    public LibGit2SharpRepositoryInfoReaderTests()
    {
        LibGit2Sharp.Repository.Init(_directory.FullName);
        _sut = new LibGit2SharpRepositoryInfoReader(_tags, _logger);
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
        var states = Fake.GetCalls(_logger)
            .Where(call => call.Method.Name == nameof(ILogger.Log))
            .Select(call => call.Arguments[2])
            .OfType<IEnumerable<KeyValuePair<string, object?>>>()
            .Select(state => state.ToDictionary(pair => pair.Key, pair => pair.Value))
            .Where(state => state.ContainsKey("ReadId"))
            .ToArray();
        states.Select(state => state["Stage"]).Should().Equal("discover", "open", "status", "head", "branches", "metadata", "repom-tags", "total");
        states.Select(state => state["ReadId"]).Distinct().Should().ContainSingle();
        states.Should().OnlyContain(state => Equals(state["Outcome"], "completed"));
        states.Select(state => state["ElapsedMs"]).OfType<double>().Should().OnlyContain(elapsed => elapsed >= 0);
    }

    [Fact]
    public async Task ReadAsync_ShouldStopAfterNativeStatus_WhenCancelled()
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

    private string[] LogMessages() => Fake.GetCalls(_logger)
        .Where(call => call.Method.Name == nameof(ILogger.Log))
        .Select(call => call.Arguments[2]!.ToString()!).ToArray();

    public void Dispose() => _directory.Delete(true);
}
