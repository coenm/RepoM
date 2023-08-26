namespace RepoM.Core.Plugin.Repository;

public sealed class RepositoryContext
{
    public RepositoryContext() : this(null)
    {
    }

    public RepositoryContext(IRepository? repository)
    {
        Repository = repository;
    }

    public IRepository? Repository { get; }
}