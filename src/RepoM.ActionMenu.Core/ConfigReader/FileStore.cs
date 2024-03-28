namespace RepoM.ActionMenu.Core.ConfigReader;

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

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
        policy.ChangeMonitors.Add(CreateMonitorForFilename(filename));
        var cacheResult = _cache.AddOrGetExisting(filename, value, policy) as T;
        return cacheResult ?? value;
    }

    private static HostFileChangeMonitor CreateMonitorForFilename(string filename)
    {
        // GitHub issue: https://github.com/coenm/RepoM/issues/88
        // the HostFileChangeMonitor relies on a real file system, not the IFileSystem used in RepoM.
        // also the HostFIleChangeMontor implements IDisposable.
        // not sure if the ObjectCache takes care of that.
        var filePaths = new List<string>(1) { filename, };
        return new HostFileChangeMonitor(filePaths);
    }
}