namespace RepoM.Api.Git;

public interface IRepositoryView
{
    string Name { get; }

    string CurrentBranch { get; }

    string Path { get; }

    bool IsPinned { get; }

    bool IsNotBare { get; }

    bool HasUnpushedChanges { get; }

    Repository Repository { get; }
}