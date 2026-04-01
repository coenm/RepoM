namespace RepoM.Core.Repositories.Favorite;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public sealed class FavoriteService : IFavoriteService, IDisposable
{
    private readonly ConcurrentDictionary<string, bool> _favoriteRepositories = new(StringComparer.OrdinalIgnoreCase);
    private readonly IFavoriteStore _favoriteStore;
    private readonly Subject<string> _saveRequested = new();
    private readonly IDisposable _saveSubscription;

    public event Action<string, bool>? FavoriteChanged;

    public FavoriteService(IFavoriteStore favoriteStore)
    {
        _favoriteStore = favoriteStore ?? throw new ArgumentNullException(nameof(favoriteStore));

        // Load initial favorites from store
        foreach (var path in _favoriteStore.Load())
        {
            _favoriteRepositories[path] = true;
        }

        // Debounce save requests: wait 2 seconds after the last change before writing
        _saveSubscription = _saveRequested
            .Throttle(TimeSpan.FromSeconds(2))
            .Subscribe(_ => PersistToStore());
    }

    public bool IsFavorite(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);
        return _favoriteRepositories.TryGetValue(safePath, out var favorite) && favorite;
    }

    public void SetFavorite(string safePath, bool favorite)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);

        bool changed;

        if (favorite)
        {
            changed = _favoriteRepositories.TryAdd(safePath, true);
            if (!changed)
            {
                // Already existed — check if value was false
                changed = _favoriteRepositories.TryUpdate(safePath, true, false);
            }
        }
        else
        {
            changed = _favoriteRepositories.TryRemove(safePath, out _);
        }

        if (changed)
        {
            FavoriteChanged?.Invoke(safePath, favorite);
            _saveRequested.OnNext(safePath);
        }
    }

    public IReadOnlyList<string> GetAllFavorites()
    {
        return _favoriteRepositories.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
    }

    private void PersistToStore()
    {
        _favoriteStore.Save(GetAllFavorites());
    }

    public void Dispose()
    {
        _saveSubscription.Dispose();
        _saveRequested.Dispose();
    }
}
