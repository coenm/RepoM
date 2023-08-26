namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.FileCache;

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

internal abstract class FileStore<T> where T : class
{
    private readonly ObjectCache _cache;

    protected FileStore(ObjectCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    internal T? Get(string filename)
    {
        if (_cache[filename] is T fileContents)
        {
            return fileContents;
        }

        return null;
    }

    internal T AddOrGetExisting(string filename, T value)
    {
        var policy = new CacheItemPolicy();
        var filePaths = new List<string>(1) { filename, };
        policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
        var cacheResult = _cache.AddOrGetExisting(filename, value, policy) as T;
        return cacheResult ?? value;
    }
}