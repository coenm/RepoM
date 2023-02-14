namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class HasUnPushedChangesMatcher : IQueryMatcher
{
    private static readonly string[] _values =
        {
            "changes",
            "unpushed-changes",
            "unpushedchanges",
        };

    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"has".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        if (_values.Contains(st.Value))
        {
            return repository.HasUnpushedChanges;
        }

        return null;
    }
}