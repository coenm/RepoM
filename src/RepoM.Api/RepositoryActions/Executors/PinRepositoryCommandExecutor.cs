namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories.Favorite;

[UsedImplicitly]
public sealed class PinRepositoryCommandExecutor : ICommandExecutor<PinRepositoryCommand>
{
    private readonly IFavoriteService _favoriteService;

    public PinRepositoryCommandExecutor(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
    }

    public void Execute(IRepository repository, PinRepositoryCommand repositoryCommand)
    {
        var newFavoriteValue = repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Pin;
        newFavoriteValue |= repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Toggle && !_favoriteService.IsFavorite(repository.SafePath);
        _favoriteService.SetFavorite(repository.SafePath, newFavoriteValue);
    }
}
