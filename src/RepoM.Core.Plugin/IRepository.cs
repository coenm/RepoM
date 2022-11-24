namespace RepoM.Core.Plugin;

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
}