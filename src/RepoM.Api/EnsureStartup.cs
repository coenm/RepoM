namespace RepoM.Api;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.Api.Resources;
using RepoM.Core.Plugin.Common;

public class EnsureStartup
{
    private readonly IFileSystem _fileSystem;
    private readonly IAppDataPathProvider _appDataProvider;

    public EnsureStartup(IFileSystem fileSystem, IAppDataPathProvider appDataProvider)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appDataProvider = appDataProvider ?? throw new ArgumentNullException(nameof(appDataProvider));
    }

    public async Task EnsureFilesAsync()
    {
        await CheckOrCreateAsync("RepositoryActionsV2.yaml", EmbeddedResources.GetRepositoryActionsV2Yaml).ConfigureAwait(false);
        await CheckOrCreateAsync("TagsV2.yaml", EmbeddedResources.GetTagsV2Yaml).ConfigureAwait(false);
        await CheckOrCreateAsync("RepoM.Filtering.yaml", EmbeddedResources.GetFilteringYaml).ConfigureAwait(false);
        await CheckOrCreateAsync("RepoM.Ordering.yaml", EmbeddedResources.GetSortingYaml).ConfigureAwait(false);
        await CheckOrCreateAsync("appsettings.serilog.json", EmbeddedResources.GetSerilogAppSettings).ConfigureAwait(false);
    }

    private async Task CheckOrCreateAsync(string filename, Func<Stream> func)
    {
        var fullFilename = Path.Combine(_appDataProvider.AppDataPath, filename);

        if (_fileSystem.File.Exists(fullFilename))
        {
            return;
        }

        await using Stream stream = func.Invoke();
        await TryCreateAsync(fullFilename, stream).ConfigureAwait(false);

        if (!_fileSystem.File.Exists(fullFilename))
        {
            throw new FileNotFoundException(fullFilename);
        }
    }

    private async Task TryCreateAsync(string filename, Stream content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms).ConfigureAwait(false);
        await _fileSystem.File.WriteAllBytesAsync(filename, ms.ToArray()).ConfigureAwait(false);
    }
}