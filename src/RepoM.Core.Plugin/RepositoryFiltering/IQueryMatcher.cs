namespace RepoM.Core.Plugin.RepositoryFiltering;

using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public interface IQueryMatcher
{
    bool? IsMatch(in IRepository repository, in TermBase term);
}