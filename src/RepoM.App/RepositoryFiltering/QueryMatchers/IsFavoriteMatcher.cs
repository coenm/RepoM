namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Favorite;

[UsedImplicitly]
public sealed class IsFavoriteMatcher : IQueryMatcher
{
    private readonly IFavoriteService _favoriteService;

    public IsFavoriteMatcher(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
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

        if ("pinned".Equals(st.Value, StringComparison.Ordinal)
            || "starred".Equals(st.Value, StringComparison.Ordinal)
            || "favorite".Equals(st.Value, StringComparison.Ordinal))
        {
            return _favoriteService.IsFavorite(repository.SafePath);
        }

        if ("unpinned".Equals(st.Value, StringComparison.Ordinal)
            || "unstarred".Equals(st.Value, StringComparison.Ordinal)
            || "unfavorite".Equals(st.Value, StringComparison.Ordinal))
        {
            return !_favoriteService.IsFavorite(repository.SafePath);
        }

        return null;
    }
}
