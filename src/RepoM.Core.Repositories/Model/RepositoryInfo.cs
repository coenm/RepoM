namespace RepoM.Core.Repositories.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RepoM.Core.Plugin.Repository;

[DebuggerDisplay("{Name} @{Path}")]
public sealed class RepositoryInfo
{
    public required string Path { get; init; }

    public required string SafePath { get; init; }

    public required string Name { get; init; }

    public string WindowsPath { get; init; } = string.Empty;

    public string LinuxPath { get; init; } = string.Empty;

    public string Location { get; init; } = string.Empty;

    public string CurrentBranch { get; set; } = string.Empty;

    public bool CurrentBranchHasUpstream { get; set; }

    public bool CurrentBranchIsDetached { get; set; }

    public bool CurrentBranchIsOnTag { get; set; }

    public string[] Branches { get; set; } = [];

    public string[] LocalBranches { get; set; } = [];

    public string[] Tags { get; set; } = [];

    public List<Remote> Remotes { get; init; } = [];

    public bool IsBare { get; init; }

    public bool HasUnpushedChanges => (AheadBy ?? 0) > 0 ||
                                      (LocalUntracked ?? 0) > 0 ||
                                      (LocalModified ?? 0) > 0 ||
                                      (LocalMissing ?? 0) > 0 ||
                                      (LocalAdded ?? 0) > 0 ||
                                      (LocalStaged ?? 0) > 0 ||
                                      (LocalRemoved ?? 0) > 0 ||
                                      (StashCount ?? 0) > 0;

    public bool HasLocalChanges => (LocalUntracked ?? 0) > 0 ||
                                   (LocalModified ?? 0) > 0 ||
                                   (LocalMissing ?? 0) > 0 ||
                                   (LocalAdded ?? 0) > 0 ||
                                   (LocalStaged ?? 0) > 0 ||
                                   (LocalRemoved ?? 0) > 0;

    public bool IsBehind => (BehindBy ?? 0) > 0;

    public int? AheadBy { get; set; }

    public int? BehindBy { get; set; }

    public int? LocalUntracked { get; set; }

    public int? LocalModified { get; set; }

    public int? LocalMissing { get; set; }

    public int? LocalAdded { get; set; }

    public int? LocalStaged { get; set; }

    public int? LocalRemoved { get; set; }

    public int? LocalIgnored { get; set; }

    public int? StashCount { get; set; }

    public Func<string[]>? AllBranchesReader { get; init; }

    public bool WasFound { get; set; } = true;

    public DateTimeOffset LastSeen { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public string GetStatusCode()
    {
        return string.Join("-",
            CurrentBranch,
            AheadBy ?? 0,
            BehindBy ?? 0,
            LocalUntracked ?? 0,
            LocalModified ?? 0,
            LocalMissing ?? 0,
            LocalAdded ?? 0,
            LocalStaged ?? 0,
            LocalRemoved ?? 0,
            LocalIgnored ?? 0,
            StashCount ?? 0);
    }
}
