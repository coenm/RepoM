namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.FileCache;

using System;
using System.IO.Abstractions;
using System.Runtime.Caching;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

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