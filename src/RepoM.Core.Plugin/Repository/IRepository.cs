namespace RepoM.Core.Plugin.Repository;

using System.Collections.Generic;

public interface IRepository
{
    string Name { get; }

    string Path { get; }

    string Location { get; }

    string CurrentBranch { get; }

    string[] Branches { get; }

    string[] LocalBranches { get; }

    string[] Tags { get; }

    string SafePath { get; }

    List<Remote> Remotes { get; }

    bool HasUnpushedChanges { get; }
}