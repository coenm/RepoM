namespace RepoM.Plugin.Demo;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Repositories.Favorite;

public sealed class DemoFavoriteStoreDecorator : IFavoriteStore
{
    private string[] _favorites = [DemoRepositories.PrimarySafePath];

    public DemoFavoriteStoreDecorator(Func<IFavoriteStore> decoratee)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
    }

    public IReadOnlyList<string> Load() => _favorites.ToArray();

    public void Save(IReadOnlyList<string> favorites)
    {
        ArgumentNullException.ThrowIfNull(favorites);
        _favorites = favorites.ToArray();
    }
}
