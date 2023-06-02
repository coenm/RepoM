namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RepoM.Core.Plugin.Repository;

[DebuggerDisplay("{Name} @{Path}")]
public class Repository : IRepository
{
    private readonly string? _normalizedPath;

    public Repository(string path)
    {
        Name = string.Empty;
        Branches = Array.Empty<string>();
        LocalBranches = Array.Empty<string>();
        CurrentBranch = string.Empty;
        Path = path;
        Location = string.Empty;
        SafePath = GetSafePath(path);
        _normalizedPath = Normalize(path);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Repository other)
        {
            return false;
        }

        if (string.IsNullOrEmpty(other._normalizedPath))
        {
            return string.IsNullOrEmpty(_normalizedPath);
        }

        return string.Equals(other._normalizedPath, _normalizedPath, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Path.GetHashCode();
    }

    public string Name { get; set; }

    public string Path { get; }

    public string Location { get; set; }

    public string CurrentBranch { get; set; }

    public string[] Branches { get; set; }

    public string[] LocalBranches { get; set; }

    public string[] Tags { get; set; } = Array.Empty<string>();

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public Func<string[]>? AllBranchesReader { get; set; }

    public string[] ReadAllBranches()
    {
        return AllBranchesReader?.Invoke() ?? Array.Empty<string>();
    }

    public bool CurrentBranchHasUpstream { get; set; }

    public bool CurrentBranchIsDetached { get; set; }

    public bool CurrentBranchIsOnTag { get; set; }

    public bool WasFound => !string.IsNullOrWhiteSpace(Path);

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

    public List<Remote> Remotes { get; } = new List<Remote>(1);

    public string SafePath { get; }
    
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

    private static string GetSafePath(string input)
    {
        // use '/' for linux systems and bash command line (will work on cmd and powershell as well)
        var safePath = input.Replace('\\', '/');
        if (safePath.EndsWith('/'))
        {
            safePath = safePath[..^1];
        }

        return safePath;
    }

    private static string? Normalize(string path)
    {
        // yeah not that beautiful but we have to add a backslash
        // or slash (depending on the OS) and on Mono, I don't have Path.PathSeparator.
        // so we add a random char with Path.Combine() and remove it again
        path = System.IO.Path.Combine(path, "_");
        path = path[..^1];

        return System.IO.Path.GetDirectoryName(path);
    }
}