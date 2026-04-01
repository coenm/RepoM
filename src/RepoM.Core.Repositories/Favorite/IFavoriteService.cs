namespace RepoM.Core.Repositories.Favorite;

using System;

public interface IFavoriteService
{
    bool IsFavorite(string safePath);

    void SetFavorite(string safePath, bool favorite);

    /// <summary>
    /// Raised when the favorite state of a repository changes.
    /// The string argument is the SafePath of the affected repository.
    /// </summary>
    event Action<string, bool>? FavoriteChanged;
}
