namespace RepoM.Core.Repositories.Pinning;

using System;

public interface IPinningService
{
    bool IsPinned(string safePath);

    void SetPinned(string safePath, bool pinned);

    /// <summary>
    /// Raised when the pinned/favorite state of a repository changes.
    /// The string argument is the SafePath of the affected repository.
    /// </summary>
    event Action<string, bool>? PinnedChanged;
}
