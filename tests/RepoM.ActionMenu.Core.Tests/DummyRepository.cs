namespace RepoM.ActionMenu.Core.Tests;

using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;

public class DummyRepository : IRepository
{
    public string SafePath => "C:/Projects/Github/RepoM";

    public List<Remote> Remotes { get; } =
        [
            new("origin", "https://github.com/coenm/RepoM.git"),
            new("dummy", "https://github.com/coenm/RepoZ"),
        ];

    public bool HasUnpushedChanges => false;

    public string Name => "dummy name";

    public bool IsBare => false;

    public string Path => WindowsPath;
    
    public string WindowsPath=> @"C:\Projects\Github\RepoM";

    public string LinuxPath => "C:/Projects/Github/RepoM";

    public string Location => "dummy location";

    public string CurrentBranch => "dummy current branch";

    public string[] Branches { get; } = [];

    public string[] LocalBranches { get; } = [];

    public string[] Tags { get; } = [];

    public string[] ReadAllBranches()
    {
        return [];
    }

    public bool HasLocalChanges => false;

    public bool IsBehind => false;
}