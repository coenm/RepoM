namespace RepoM.Api.Git;

using System;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;

public class DefaultRepositoryStore : FileRepositoryStore
{
    private readonly string _fullFilename;

    public DefaultRepositoryStore(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger)
        : base(fileSystem, logger)
    {
        ArgumentNullException.ThrowIfNull(appDataPathProvider);
        _fullFilename = Path.Combine(appDataPathProvider.AppDataPath, "Repositories.cache");
    }

    protected override string GetFileName()
    {
        return _fullFilename;
    }
}