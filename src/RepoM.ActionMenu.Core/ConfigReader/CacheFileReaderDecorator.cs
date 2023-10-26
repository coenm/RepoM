namespace RepoM.ActionMenu.Core.ConfigReader;

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using DotNetEnv;
using RepoM.ActionMenu.Core.Yaml.Model;

internal class FileStore<T> where T : class
{
    private readonly ObjectCache _cache;

    internal FileStore(ObjectCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public T? Get(string filename)
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

internal class CacheFileReaderDecorator : IFileReader
{
    private readonly IFileReader _decoratee;
    private readonly FileStore<Dictionary<string, string>> _envStore;
    private readonly FileStore<ContextRoot> _contextRootStore;
    private readonly FileStore<Root> _rootStore;

    public CacheFileReaderDecorator(IFileReader decoratee)
    {
        var cache = new MemoryCache(nameof(CacheFileReaderDecorator));
        _decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        _envStore = new FileStore<Dictionary<string, string>>(cache);
        _contextRootStore = new FileStore<ContextRoot>(cache);
        _rootStore = new FileStore<Root>(cache);
    }

    public async Task<Root?> DeserializeRoot(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }

        Root? result = _rootStore.Get(filename);
        if (result != null)
        {
            return result;
        }

        result = await _decoratee.DeserializeRoot(filename).ConfigureAwait(false);
        if (result == null)
        {
            return result;
        }

        return _rootStore.AddOrGetExisting(filename, result);
    }

    public async Task<ContextRoot?> DeserializeContextRoot(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }

        ContextRoot? result = _contextRootStore.Get(filename);
        if (result != null)
        {
            return result;
        }

        result = await _decoratee.DeserializeContextRoot(filename).ConfigureAwait(false);
        if (result == null)
        {
            return result;
        }

        return _contextRootStore.AddOrGetExisting(filename, result);
    }

    public async Task<IDictionary<string, string>?> ReadEnvAsync(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }
        
        IDictionary<string, string>? result = _envStore.Get(filename);
        if (result != null)
        {
            return result;
        }

        result = await _decoratee.ReadEnvAsync(filename).ConfigureAwait(false);
        if (result == null)
        {
            return result;
        }

        return _envStore.AddOrGetExisting(filename, result.ToDictionary());
    }
}