namespace RepoM.App.RepositoryFiltering;

using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public interface IRepositoryMatcher
{
    bool Matches(IRepository repository, IQuery query);
}