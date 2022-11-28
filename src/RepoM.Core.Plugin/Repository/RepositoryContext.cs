namespace RepoM.Core.Plugin.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RepositoryContext
{
    public RepositoryContext()
    {
        Repositories = Array.Empty<IRepository>();
    }

    public RepositoryContext(params IRepository[] repositories)
    {
        Repositories = repositories.ToArray();
    }

    public RepositoryContext(IEnumerable<IRepository> repositories)
    {
        Repositories = repositories.ToArray();
    }

    public IRepository[] Repositories { get; }
}