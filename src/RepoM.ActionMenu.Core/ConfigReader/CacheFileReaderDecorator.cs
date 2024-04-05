namespace RepoM.ActionMenu.Core.ConfigReader;

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Core.Yaml.Model;

[UsedImplicitly]
internal class CacheFileReaderDecorator : IFileReader
{
    private readonly IFileReader _decoratee;
    private readonly FileStore<IDictionary<string, string>> _envStore;
    private readonly FileStore<ContextRoot> _contextRootStore;
    private readonly FileStore<ActionMenuRoot> _rootStore;
    private readonly FileStore<TagsRoot> _tagsRootStore;

    public CacheFileReaderDecorator(IFileReader decoratee)
    {
        var cache = new MemoryCache(nameof(CacheFileReaderDecorator));
        _decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        _envStore = new FileStore<IDictionary<string, string>>(cache);
        _contextRootStore = new FileStore<ContextRoot>(cache);
        _rootStore = new FileStore<ActionMenuRoot>(cache);
        _tagsRootStore = new FileStore<TagsRoot>(cache);
    }

    public Task<ActionMenuRoot?> DeserializeRoot(string filename)
    {
        return DeserializeAsync(filename, _rootStore, _decoratee.DeserializeRoot);
    }

    public Task<TagsRoot?> DeserializeTagsRoot(string filename)
    {
        return DeserializeAsync(filename, _tagsRootStore, _decoratee.DeserializeTagsRoot);
    }

    public Task<ContextRoot?> DeserializeContextRoot(string filename)
    {
        return DeserializeAsync(filename, _contextRootStore, _decoratee.DeserializeContextRoot);
    }

    public Task<IDictionary<string, string>?> ReadEnvAsync(string filename)
    {
        return DeserializeAsync(filename, _envStore, _decoratee.ReadEnvAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<T?> DeserializeAsync<T>(string filename, FileStore<T> store, Func<string, Task<T?>> decorateeFunc) where T : class
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }

        T? result = store.Get(filename);
        if (result != null)
        {
            return result;
        }

        result = await decorateeFunc.Invoke(filename).ConfigureAwait(false);
        if (result == null)
        {
            return result;
        }

        return store.AddOrGetExisting(filename, result);
    }
}