namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Caching;
using DotNetEnv;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

internal class RepositoryActionsFileStore
{
    private readonly IFileSystem _fileSystem;
    private readonly IRepositoryActionDeserializer _repositoryActionsDeserializer;
    private readonly ObjectCache _cache;

    public RepositoryActionsFileStore(IFileSystem fileSystem, IRepositoryActionDeserializer repositoryActionsDeserializer, ObjectCache cache)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repositoryActionsDeserializer = repositoryActionsDeserializer ?? throw new ArgumentNullException(nameof(repositoryActionsDeserializer));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public RepositoryActionConfiguration? TryGet(string filename)
    {
        if (_cache[filename] is RepositoryActionConfiguration fileContents)
        {
            return fileContents;
        }

        var policy = new CacheItemPolicy();
        var filePaths = new List<string>(1) { filename, };
        policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

        var payload = _fileSystem.File.ReadAllText(filename);
        fileContents = _repositoryActionsDeserializer.Deserialize(payload);

        if (fileContents == null)
        {
            return null;
        }

        var cacheResult = _cache.AddOrGetExisting(filename, fileContents, policy) as RepositoryActionConfiguration;
        return cacheResult ?? fileContents;
    }
}

internal class EnvFileStore
{
    private readonly ObjectCache _cache;

    public EnvFileStore(ObjectCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public IDictionary<string, string> TryGet(string filename)
    {
        if (_cache[filename] is Dictionary<string, string> fileContents)
        {
            return fileContents;
        }

        var policy = new CacheItemPolicy();
        var filePaths = new List<string>(1) { filename, };
        policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

        IEnumerable<KeyValuePair<string, string>>? result = Env.Load(filename, new LoadOptions(setEnvVars: false));

        fileContents = result == null ? new Dictionary<string, string>(0) : result.ToDictionary();

        var cacheResult = _cache.AddOrGetExisting(filename, fileContents, policy) as Dictionary<string, string>;
        return cacheResult ?? fileContents;
    }
}