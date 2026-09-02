namespace RepoM.Core.Repositories.Persistence;

using System;

public sealed record RepositorySnapshotStoreSettings
{
    public RepositorySnapshotStoreSettings(string filePath)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath)
            ? throw new ArgumentException("File path must not be empty.", nameof(filePath))
            : filePath;
    }

    public string FilePath { get; }
}
