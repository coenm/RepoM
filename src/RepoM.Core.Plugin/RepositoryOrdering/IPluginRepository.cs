namespace RepoM.Core.Plugin.RepositoryOrdering;

public interface IPluginRepository
{
    string Name { get; }

    string CurrentBranch { get; }

    string Path { get; }

    bool IsPinned { get; }

    bool HasUnpushedChanges { get; }
}