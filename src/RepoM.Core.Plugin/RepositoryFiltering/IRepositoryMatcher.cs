namespace RepoM.Core.Plugin.RepositoryFiltering;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public interface IRepositoryMatcher
{
    public bool? IsMatch(IRepository repository, IQuery query);
}

internal class RepositoryMatcher : IRepositoryMatcher
{
    public bool? IsMatch(IRepository repository, IQuery query)
    {
        return true;
    }
}