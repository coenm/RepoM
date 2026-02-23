namespace RepoM.Core.Repositories.Pinning;

public interface IPinningService
{
    bool IsPinned(string safePath);

    void SetPinned(string safePath, bool pinned);
}
