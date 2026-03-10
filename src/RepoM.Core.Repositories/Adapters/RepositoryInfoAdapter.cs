namespace RepoM.Core.Repositories.Adapters;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Model;

public sealed class RepositoryInfoAdapter : IRepository
{
    private RepositoryInfo _info;

    public RepositoryInfoAdapter(RepositoryInfo info)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
    }

    public RepositoryInfo RepositoryInfo => _info;

    public void UpdateInfo(RepositoryInfo info)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
    }

    public string Name => _info.Name;

    public bool IsBare => _info.IsBare;

    public string Path => _info.Path;

    public string WindowsPath => _info.WindowsPath;

    public string LinuxPath => _info.LinuxPath;

    public string Location => _info.Location;

    public string CurrentBranch => _info.CurrentBranch;

    public string[] Branches => _info.Branches;

    public string[] LocalBranches => _info.LocalBranches;

    public string[] Tags => _info.Tags;

    public string SafePath => _info.SafePath;

    public List<Remote> Remotes => _info.Remotes;

    public bool HasUnpushedChanges => _info.HasUnpushedChanges;

    public bool HasLocalChanges => _info.HasLocalChanges;

    public bool IsBehind => _info.IsBehind;

    public string[] ReadAllBranches()
    {
        return _info.AllBranchesReader?.Invoke() ?? [];
    }
}
