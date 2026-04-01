namespace RepoM.Api.Ordering.IsFavorite;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Repositories.Favorite;

public sealed class IsFavoriteScoreCalculator : IRepositoryScoreCalculator
{
    private readonly IFavoriteService _favoriteService;
    private readonly int _weight;

    public IsFavoriteScoreCalculator(IFavoriteService favoriteService, int weight)
    {
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
        _weight = weight;
    }

    public int Score(IRepository repository)
    {
        return _favoriteService.IsFavorite(repository.SafePath) ? _weight : 0;
    }
}
