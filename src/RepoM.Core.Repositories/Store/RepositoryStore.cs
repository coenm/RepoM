namespace RepoM.Core.Repositories.Store;

using System;
using System.Collections.Generic;
using DynamicData;
using DynamicData.Kernel;
using RepoM.Core.Repositories.Model;

public sealed class RepositoryStore : IRepositoryStore
{
    private readonly SourceCache<RepositoryInfo, string> _cache;
    private bool _disposed;

    public RepositoryStore()
    {
        _cache = new SourceCache<RepositoryInfo, string>(r => r.SafePath);
    }

    public void AddOrUpdate(RepositoryInfo repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        Optional<RepositoryInfo> existing = _cache.Lookup(repository.SafePath);
        if (existing.HasValue && existing.Value.Equals(repository))
        {
            return;
        }

        _cache.AddOrUpdate(repository);
    }

    public void AddOrUpdateRange(IEnumerable<RepositoryInfo> repositories)
    {
        ArgumentNullException.ThrowIfNull(repositories);
        _cache.Edit(updater =>
        {
            foreach (RepositoryInfo repo in repositories)
            {
                Optional<RepositoryInfo> existing = updater.Lookup(repo.SafePath);
                if (existing.HasValue && existing.Value.Equals(repo))
                {
                    continue;
                }

                updater.AddOrUpdate(repo);
            }
        });
    }

    public void Remove(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);
        _cache.RemoveKey(safePath);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public IObservable<IChangeSet<RepositoryInfo, string>> Connect()
    {
        return _cache.Connect();
    }

    public IObservable<IChangeSet<RepositoryInfo, string>> Connect(Func<RepositoryInfo, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _cache.Connect().Filter(predicate);
    }

    public Optional<RepositoryInfo> Lookup(string safePath)
    {
        return _cache.Lookup(safePath);
    }

    public int Count => _cache.Count;

    public IEnumerable<RepositoryInfo> Items => _cache.Items;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cache.Dispose();
    }
}
