namespace RepoM.App.Tests.Controls;

using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using RepoM.UI.Controls;
using Xunit;

public class RepositoryListLoadingTests
{
    [Fact]
    public async Task WaitForContextMenuLoadingIndicatorAsync_ShouldHideIndicatorWhenMenuIsReadyInTime()
    {
        var shouldShow = await RepositoryList.WaitForContextMenuLoadingIndicatorAsync(
            Task.CompletedTask, 200, CancellationToken.None);

        shouldShow.Should().BeFalse();
    }

    [Fact]
    public async Task WaitForContextMenuLoadingIndicatorAsync_ShouldShowIndicatorAfterConfiguredDelay()
    {
        var entries = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var shouldShow = await RepositoryList.WaitForContextMenuLoadingIndicatorAsync(
            entries.Task, 20, CancellationToken.None);

        shouldShow.Should().BeTrue();
        entries.SetResult();
    }

    [Fact]
    public async Task WaitForContextMenuLoadingIndicatorAsync_ShouldSuppressIndicatorForNegativeDelay()
    {
        var entries = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var shouldShow = await RepositoryList.WaitForContextMenuLoadingIndicatorAsync(
            entries.Task, -1, CancellationToken.None);

        shouldShow.Should().BeFalse();
        entries.SetResult();
    }

    [Fact]
    public async Task WaitForContextMenuLoadingIndicatorAsync_ShouldCancelPendingDelay()
    {
        using var cancellation = new CancellationTokenSource();
        var entries = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var waiting = RepositoryList.WaitForContextMenuLoadingIndicatorAsync(
            entries.Task, 5000, cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        entries.SetResult();
    }
}
