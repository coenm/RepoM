namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Caching;
using DotNetEnv;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

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

internal class RepositoryActionsFileStore : FileStore<RepositoryActionConfiguration>
{
    private readonly IFileSystem _fileSystem;
    private readonly IRepositoryActionDeserializer _repositoryActionsDeserializer;


    public RepositoryActionsFileStore(IFileSystem fileSystem, IRepositoryActionDeserializer repositoryActionsDeserializer, ObjectCache cache) : base(cache)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repositoryActionsDeserializer = repositoryActionsDeserializer ?? throw new ArgumentNullException(nameof(repositoryActionsDeserializer));
    }

    public RepositoryActionConfiguration? TryGet(string filename)
    {
        RepositoryActionConfiguration? result = Get(filename);

        if (result != null)
        {
            return result;
        }

        var payload = _fileSystem.File.ReadAllText(filename);
        RepositoryActionConfiguration? fileContents = _repositoryActionsDeserializer.Deserialize(payload);

        if (fileContents == null)
        {
            return null;
        }

        return AddOrGetExisting(filename, fileContents);
    }
}

internal class EnvFileStore : FileStore<Dictionary<string, string>>
{
    public EnvFileStore(ObjectCache cache) : base(cache)
    {
    }

    public IDictionary<string, string> TryGet(string filename)
    {
        Dictionary<string, string>? result = Get(filename);

        if (result != null)
        {
            return result;
        }

        IEnumerable<KeyValuePair<string, string>>? envResult = Env.Load(filename, new LoadOptions(setEnvVars: false));

        Dictionary<string, string>? fileContents = envResult == null ? new Dictionary<string, string>(0) : envResult.ToDictionary();

        return AddOrGetExisting(filename, fileContents);
    }
}