namespace RepoM.Core.Plugin.Repository;

public sealed class RepositoryContext
{
    public RepositoryContext()
    {
        Repository = null;
    }

    public RepositoryContext(IRepository repository)
    {
        Repository = repository;
    }

    public static RepositoryContext EmptyContext { get; } = new RepositoryContext();

    public IRepository? Repository { get; }

    public static RepositoryContext Create(IRepository? repository)
    {
        return repository == null ? EmptyContext : new RepositoryContext(repository);
    }
}