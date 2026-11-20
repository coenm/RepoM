namespace RepoM.StartupPlugin.Demo;

using System;
using System.Collections.Generic;
using RepoM.Core.Repositories.Model;

internal static class DemoRepositories
{
    internal const string PrimarySafePath = "C:/RepoM-Demo/RepoM";

    internal static IReadOnlyList<RepositoryInfo> Create() =>
    [
        CreateRepository("RepoM", "feature/demo", ["desktop", "C#"], ahead: 2, behind: 1, modified: 3),
        CreateRepository("Documentation", "docs/screenshots", ["documentation"], behind: 3),
        CreateRepository("WebShop", "main", ["web", "services"]),
        CreateRepository("Experiments", "feature/new-idea", ["personal"], upstream: false, modified: 1),
        new RepositoryInfo
        {
            Name = "Archived repository",
            Path = @"C:\RepoM-Demo\Archive",
            SafePath = "C:/RepoM-Demo/Archive",
            CurrentBranch = "main",
            WasFound = false,
            LastSeen = DateTimeOffset.UnixEpoch,
            LastUpdated = DateTimeOffset.UnixEpoch,
        },
    ];

    private static RepositoryInfo CreateRepository(string name, string branch, string[] tags,
        int ahead = 0, int behind = 0, int modified = 0, bool upstream = true) => new()
    {
        Name = name,
        Path = $@"C:\RepoM-Demo\{name}",
        SafePath = $"C:/RepoM-Demo/{name}",
        WindowsPath = $@"C:\RepoM-Demo\{name}",
        LinuxPath = $"C:/RepoM-Demo/{name}",
        CurrentBranch = branch,
        CurrentBranchHasUpstream = upstream,
        Branches = branch == "main" ? ["main", "develop"] : ["main", branch],
        LocalBranches = branch == "main" ? ["main", "develop"] : ["main", branch],
        Tags = tags,
        AheadBy = ahead,
        BehindBy = behind,
        LocalModified = modified,
        LocalUntracked = 0,
        LocalAdded = 0,
        LocalStaged = 0,
        LocalMissing = 0,
        LocalRemoved = 0,
        StashCount = 0,
        LastSeen = DateTimeOffset.UnixEpoch,
        LastUpdated = DateTimeOffset.UnixEpoch,
    };
}
