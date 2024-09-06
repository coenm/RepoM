namespace RepoM.Api.Git;

using System;
using System.IO;
using System.IO.Abstractions;
using RepoM.Core.Plugin.Common;

public class DefaultRepositoryStore : FileRepositoryStore
{
    private readonly string _fullFilename;

    public DefaultRepositoryStore(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem)
        : base(fileSystem)
    {
        AppDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));


#if DEBUG
        _fullFilename = Path.Combine(AppDataPathProvider.AppDataPath, "Repositories.cache.debug");
#else
       _fullFilename = Path.Combine(AppDataPathProvider.AppDataPath, "Repositories.cache");
#endif
    }

    protected override string GetFileName()
    {
        return _fullFilename;
    }

    public IAppDataPathProvider AppDataPathProvider { get; }
}
