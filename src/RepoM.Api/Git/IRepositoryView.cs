namespace RepoM.Api.Git;

using RepoM.Core.Plugin.Repository;

public interface IRepositoryView
{
    string Name { get; }

    string CurrentBranch { get; }

    string Path { get; }

    bool IsFavorite { get; }

    bool IsNotBare { get; }

    bool HasUnpushedChanges { get; }

    IRepository Repository { get; }
}
