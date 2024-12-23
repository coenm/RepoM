namespace RepoM.Core.Plugin.Repository;

using System.Collections.Generic;

/// <summary>
/// Git Repository entity.
/// </summary>
public interface IRepository
{
    string Name { get; }

    bool IsBare { get; }

    string Path { get; }

    string WindowsPath { get; }

    string LinuxPath { get; }

    string Location { get; }

    string CurrentBranch { get; }

    string[] Branches { get; }

    string[] LocalBranches { get; }

    string[] Tags { get; }

    string SafePath { get; }

    List<Remote> Remotes { get; }

    bool HasUnpushedChanges { get; }

    public string[] ReadAllBranches();
    
    public bool HasLocalChanges { get; }

    public bool IsBehind { get; }
}