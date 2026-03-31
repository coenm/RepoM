namespace RepoM.Core.Repositories.Pinning;

using System.Collections.Generic;

public interface IFavoritesStore
{
    IReadOnlyList<string> Load();

    void Save(IReadOnlyList<string> favorites);
}
