namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class IsPinnedMatcher : IQueryMatcher
{
    private readonly IRepositoryMonitor _monitor;

    public IsPinnedMatcher(IRepositoryMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

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

        if ("pinned".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return _monitor.IsPinned(repository);
        }

        if ("unpinned".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return !_monitor.IsPinned(repository);
        }

        return null;
    }
}