namespace RepoM.Api.IO.ExpressionEvaluator;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;

public class RepositoryContext
{
    public RepositoryContext()
    {
        Repositories = Array.Empty<Repository>();
    }

    public RepositoryContext(params Repository[] repositories)
    {
        Repositories = repositories.ToArray();
    }

    public RepositoryContext(IEnumerable<Repository> repositories)
    {
        Repositories = repositories.ToArray();
    }

    public Repository[] Repositories { get; }
}