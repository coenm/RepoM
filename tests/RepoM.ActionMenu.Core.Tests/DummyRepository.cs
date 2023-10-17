namespace RepoM.ActionMenu.Core.Tests;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;

public class DummyRepository : IRepository
{
    public string SafePath { get; } = "dummy safe path";
    public List<Remote> Remotes { get; } = new List<Remote>();
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