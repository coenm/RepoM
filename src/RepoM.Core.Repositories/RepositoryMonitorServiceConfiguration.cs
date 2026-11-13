namespace RepoM.Core.Repositories;

using System;

public sealed record RepositoryMonitorServiceConfiguration
{
    public RepositoryMonitorServiceConfiguration(TimeSpan snapshotSaveDebounceInterval)
    {
        if (snapshotSaveDebounceInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(snapshotSaveDebounceInterval));
        }

        SnapshotSaveDebounceInterval = snapshotSaveDebounceInterval;
    }

    public TimeSpan SnapshotSaveDebounceInterval { get; }
}
