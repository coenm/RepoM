namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Pinning;

[UsedImplicitly]
public sealed class IsPinnedMatcher : IQueryMatcher
{
    private readonly IPinningService _pinningService;

    public IsPinnedMatcher(IPinningService pinningService)
    {
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
    }

    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"is".Equals(st.Term, StringComparison.Ordinal))
        {
            return null;
        }

        if ("pinned".Equals(st.Value, StringComparison.Ordinal))
        {
            return _pinningService.IsPinned(repository.SafePath);
        }

        if ("unpinned".Equals(st.Value, StringComparison.Ordinal))
        {
            return !_pinningService.IsPinned(repository.SafePath);
        }

        return null;
    }
}
