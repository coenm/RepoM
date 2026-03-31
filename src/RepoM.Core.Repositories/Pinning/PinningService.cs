namespace RepoM.Core.Repositories.Pinning;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public sealed class PinningService : IPinningService, IDisposable
{
    private readonly ConcurrentDictionary<string, bool> _pinnedRepositories = new(StringComparer.OrdinalIgnoreCase);
    private readonly IFavoritesStore _favoritesStore;
    private readonly Subject<string> _saveRequested = new();
    private readonly IDisposable _saveSubscription;

    public event Action<string, bool>? PinnedChanged;

    public PinningService(IFavoritesStore favoritesStore)
    {
        _favoritesStore = favoritesStore ?? throw new ArgumentNullException(nameof(favoritesStore));

        // Load initial favorites from store
        foreach (var path in _favoritesStore.Load())
        {
            _pinnedRepositories[path] = true;
        }

        // Debounce save requests: wait 2 seconds after the last change before writing
        _saveSubscription = _saveRequested
            .Throttle(TimeSpan.FromSeconds(2))
            .Subscribe(_ => PersistToStore());
    }

    public bool IsPinned(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);
        return _pinnedRepositories.TryGetValue(safePath, out var pinned) && pinned;
    }

    public void SetPinned(string safePath, bool pinned)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);

        bool changed;

        if (pinned)
        {
            changed = _pinnedRepositories.TryAdd(safePath, true);
            if (!changed)
            {
                // Already existed — check if value was false
                changed = _pinnedRepositories.TryUpdate(safePath, true, false);
            }
        }
        else
        {
            changed = _pinnedRepositories.TryRemove(safePath, out _);
        }

        if (changed)
        {
            PinnedChanged?.Invoke(safePath, pinned);
            _saveRequested.OnNext(safePath);
        }
    }

    public IReadOnlyList<string> GetAllPinned()
    {
        return _pinnedRepositories.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
    }

    private void PersistToStore()
    {
        _favoritesStore.Save(GetAllPinned());
    }

    public void Dispose()
    {
        _saveSubscription.Dispose();
        _saveRequested.Dispose();
    }
}
