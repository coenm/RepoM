namespace RepoM.Core.Repositories.Favorite;

using System.Collections.Generic;

public interface IFavoriteStore
{
    IReadOnlyList<string> Load();

    void Save(IReadOnlyList<string> favorites);
}
