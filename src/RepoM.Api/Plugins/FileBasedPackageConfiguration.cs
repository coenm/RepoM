namespace RepoM.Api.Plugins;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;

internal class FileBasedPackageConfiguration : IPackageConfiguration
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _filename;

    public FileBasedPackageConfiguration(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger, string filename)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
    }

    public async Task<int?> GetConfigurationVersionAsync()
    {
        ConfigEnvelope<object>? result = await LoadAsync<object>().ConfigureAwait(false);
        return result?.Version;
    }

    public async Task<T?> LoadConfigurationAsync<T>() where T : class, new()
    {
        ConfigEnvelope<T>? result = await LoadAsync<T>().ConfigureAwait(false);
        return result?.Settings;
    }

    public async Task PersistConfigurationAsync<T>(T configuration, int version)
    {
        if (configuration == null)
        {
            return;
        }

        var filename = GetFilename();

        var exists = MakeSureDirectoryExists(filename);
        if (!exists)
        {
            return;
        }

        var json = JsonConvert.SerializeObject(new ConfigEnvelope<T> { Version = version, Settings = configuration, }, Formatting.Indented);

        try
        {
            await _fileSystem.File.WriteAllTextAsync(filename, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not persist configuration {message}", e.Message);
        }
    }

    private bool MakeSureDirectoryExists(string filename)
    {
        try
        {
            IFileInfo fi = _fileSystem.FileInfo.New(filename);
            var directoryName = fi.Directory?.FullName;

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                return false;
            }

            if (_fileSystem.Directory.Exists(directoryName))
            {
                return true;
            }

            try
            {
                _fileSystem.Directory.CreateDirectory(directoryName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not create directory '{directoryName}'. {message}", directoryName, e.Message);
            }

            return _fileSystem.Directory.Exists(directoryName);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<ConfigEnvelope<T>?> LoadAsync<T>()
    {
        var filename = GetFilename();
        if (!_fileSystem.File.Exists(filename))
        {
            return null;
        }

        try
        {
            var json = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            ConfigEnvelope<T>? result = JsonConvert.DeserializeObject<ConfigEnvelope<T>>(json);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not deserialize '{filename}'", filename);
            return null;
        }
    }

    private string GetFilename()
    {
        return Path.Combine(_appDataPathProvider.AppDataPath, "Module", _filename + ".json");
    }

    private sealed class ConfigEnvelope<T>
    {
        public int? Version { get; init; }

        public T? Settings { get; init; }
    }
}