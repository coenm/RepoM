namespace RepoM.Core.Repositories.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RepoM.Core.Plugin.Repository;

[DebuggerDisplay("{Name} @{Path}")]
public sealed class RepositoryInfo : IEquatable<RepositoryInfo>
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

    public int? StashCount { get; set; }

    public Func<string[]>? AllBranchesReader { get; init; }

    public bool WasFound { get; set; } = true;

    public DateTimeOffset LastSeen { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public int GetStatusCode()
    {
        var hash = new HashCode();
        hash.Add(CurrentBranch);
        hash.Add(AheadBy ?? 0);
        hash.Add(BehindBy ?? 0);
        hash.Add(LocalUntracked ?? 0);
        hash.Add(LocalModified ?? 0);
        hash.Add(LocalMissing ?? 0);
        hash.Add(LocalAdded ?? 0);
        hash.Add(LocalStaged ?? 0);
        hash.Add(LocalRemoved ?? 0);
        hash.Add(StashCount ?? 0);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares observable repository state, intentionally excluding
    /// <see cref="LastSeen"/>, <see cref="LastUpdated"/>, and <see cref="AllBranchesReader"/>
    /// so that two snapshots of the same repo are considered equal when nothing the UI cares about has changed.
    /// </summary>
    public bool Equals(RepositoryInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return SafePath == other.SafePath
            && CurrentBranch == other.CurrentBranch
            && CurrentBranchHasUpstream == other.CurrentBranchHasUpstream
            && CurrentBranchIsDetached == other.CurrentBranchIsDetached
            && CurrentBranchIsOnTag == other.CurrentBranchIsOnTag
            && AheadBy == other.AheadBy
            && BehindBy == other.BehindBy
            && LocalUntracked == other.LocalUntracked
            && LocalModified == other.LocalModified
            && LocalMissing == other.LocalMissing
            && LocalAdded == other.LocalAdded
            && LocalStaged == other.LocalStaged
            && LocalRemoved == other.LocalRemoved
            && StashCount == other.StashCount
            && WasFound == other.WasFound
            && Tags.SequenceEqual(other.Tags)
            && Branches.SequenceEqual(other.Branches)
            && LocalBranches.SequenceEqual(other.LocalBranches);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as RepositoryInfo);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SafePath, CurrentBranch, AheadBy, BehindBy, StashCount);
    }
}
