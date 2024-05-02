namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class IsBehindMatcher : IQueryMatcher
{
    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"is".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        if (!"behind".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return null;
        }

        return repository.IsBehind;
    }
}