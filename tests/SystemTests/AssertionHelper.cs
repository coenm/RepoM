namespace SystemTests;

using System;
using System.Threading;
using FluentAssertions;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;

internal static class AssertionHelper
{
    public static void Expect(this DefaultRepositoryMonitor monitor, Action act, int changes, int deletes)
    {
        ExpectInternal(monitor, act, out var actualChanges, out var actualDeletes);

        actualChanges.Should().Be(changes);
        actualDeletes.Should().Be(deletes);
    }

    public static void Expect(
        this DefaultRepositoryMonitor monitor,
        Action act,
        Func<int, bool> changesAssertion,
        Func<int, bool> deletesAssertion)
    {
        ExpectInternal(monitor, act, out var actualChanges, out var actualDeletes);

        changesAssertion(actualChanges).Should().BeTrue();
        deletesAssertion(actualDeletes).Should().BeTrue();
    }

    private static void ExpectInternal(
        this DefaultRepositoryMonitor monitor,
        Action act,
        out int actualChanges,
        out int actualDeletes)
    {
        var trackedChanges = 0;
        var trackedDeletes = 0;
        EventHandler<IRepository> trackedChangesInc = (s, e) => trackedChanges++;
        EventHandler<string> trackedDeletesInc = (s, e) => trackedDeletes++;

        try
        {
            monitor.OnChangeDetected += trackedChangesInc;
            monitor.OnDeletionDetected += trackedDeletesInc;

            monitor.Observe();

            act();

            // let's be generous
            var delay = 3 * monitor.DelayGitStatusAfterFileOperationMilliseconds;
            Thread.Sleep(delay);
        }
        finally
        {
            monitor.Stop();

            monitor.OnChangeDetected -= trackedChangesInc;
            monitor.OnDeletionDetected -= trackedDeletesInc;
        }

        actualChanges = trackedChanges;
        actualDeletes = trackedDeletes;
    }
}