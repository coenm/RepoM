namespace RepoM.Core.Repositories.Pinning;

using System;
using System.Collections.Concurrent;

public sealed class PinningService : IPinningService
{
    private readonly ConcurrentDictionary<string, bool> _pinnedRepositories = new(StringComparer.OrdinalIgnoreCase);

    public bool IsPinned(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);
        return _pinnedRepositories.TryGetValue(safePath, out var pinned) && pinned;
    }

    public void SetPinned(string safePath, bool pinned)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);

        if (pinned)
        {
            _pinnedRepositories[safePath] = true;
        }
        else
        {
            _pinnedRepositories.TryRemove(safePath, out _);
        }
    }
}
