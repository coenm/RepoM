namespace RepoM.Core.Repositories.Model;

public record RepositoryChangeEvent(string Path, RepositoryChangeType ChangeType);

public enum RepositoryChangeType
{
    Added,
    Modified,
    Removed,
}
