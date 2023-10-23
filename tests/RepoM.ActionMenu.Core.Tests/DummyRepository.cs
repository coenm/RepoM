namespace RepoM.ActionMenu.Core.Tests;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;

public class DummyRepository : IRepository
{
    public string SafePath { get; } = "C:/Projects/Github/RepoM/RepoM.git";

    public List<Remote> Remotes { get; } = new List<Remote>()
        {
            new Remote("origin", "https://github.com/coenm/RepoM.git"),
            new Remote("dummy", "https://github.com/coenm/RepoZ"),
        };
    public bool HasUnpushedChanges { get; } = false;
    public string Name { get; } = "dummy name";
    public string Path { get; } = "dummy path";
    public string Location { get; } = "dummy location";
    public string CurrentBranch { get; } = "dummy current branch";
    public string[] Branches { get; } = Array.Empty<string>();
    public string[] LocalBranches { get; } = Array.Empty<string>();
    public string[] Tags { get; } = Array.Empty<string>();

    public string[] ReadAllBranches()
    {
        return Array.Empty<string>();
    }
}