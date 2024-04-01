namespace RepoM.Api;

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;

public class EnsureStartup
{
    private readonly IFileSystem _fileSystem;
    private readonly IAppDataPathProvider _appDataProvider;
    private readonly ILogger _logger;

    public EnsureStartup(IFileSystem fileSystem, IAppDataPathProvider appDataProvider, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appDataProvider = appDataProvider ?? throw new ArgumentNullException(nameof(appDataProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureFilesAsync()
    {
        var filename = Path.Combine(_appDataProvider.AppDataPath, "RepositoryActionsV2.yaml");

        if (!_fileSystem.File.Exists(filename))
        {
            await TryCreateAsync(filename, new MemoryStream()).ConfigureAwait(false);
        }

        if (!_fileSystem.File.Exists(filename))
        {
            throw new FileNotFoundException(filename);
        }
    }

    private async Task TryCreateAsync(string filename, Stream content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms).ConfigureAwait(false);
        await _fileSystem.File.WriteAllBytesAsync(filename, ms.ToArray()).ConfigureAwait(false);
    }
}