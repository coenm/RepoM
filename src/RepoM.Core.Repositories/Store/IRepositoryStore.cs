namespace RepoM.Core.Repositories.Store;

using System;
using System.Collections.Generic;
using DynamicData;
using DynamicData.Kernel;
using RepoM.Core.Repositories.Model;

public interface IRepositoryStore : IDisposable
{
    void AddOrUpdate(RepositoryInfo repository);

    void AddOrUpdateRange(IEnumerable<RepositoryInfo> repositories);

    void Remove(string safePath);

    void Clear();

    IObservable<IChangeSet<RepositoryInfo, string>> Connect();

    IObservable<IChangeSet<RepositoryInfo, string>> Connect(Func<RepositoryInfo, bool> predicate);

    Optional<RepositoryInfo> Lookup(string safePath);

    int Count { get; }

    IEnumerable<RepositoryInfo> Items { get; }
}
