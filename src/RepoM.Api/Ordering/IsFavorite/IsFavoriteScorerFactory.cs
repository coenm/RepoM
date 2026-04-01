namespace RepoM.Api.Ordering.IsFavorite;

using System;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Repositories.Favorite;

public class IsFavoriteScorerFactory : IRepositoryScoreCalculatorFactory<IsFavoriteScorerConfigurationV1>
{
    private readonly IFavoriteService _favoriteService;

    public IsFavoriteScorerFactory(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
    }

    public IRepositoryScoreCalculator Create(IsFavoriteScorerConfigurationV1 config)
    {
        return new IsFavoriteScoreCalculator(_favoriteService, config.Weight);
    }
}
