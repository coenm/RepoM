namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryFiltering.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FilesFilterSettingsService : IFilterSettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private Dictionary<string, RepositoryFilterConfiguration>? _configuration;
    
    public FilesFilterSettingsService(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
    }

    public Dictionary<string, RepositoryFilterConfiguration> Configuration => _configuration ??= Load();

    private string GetFileName()
    {
        return _fileSystem.Path.Combine(_appDataPathProvider.GetAppDataPath(), "RepoM.Filtering.yaml");
    }

    private Dictionary<string, RepositoryFilterConfiguration> Load()
    {
        var file = GetFileName();

        if (!_fileSystem.File.Exists(file))
        {
            throw new FileNotFoundException("Comparer configuration file not found", file);
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(file);

            DeserializerBuilder builder = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance);

            IDeserializer deserializer = builder.Build();

            Dictionary<string, RepositoryFilterConfiguration> result = deserializer.Deserialize<Dictionary<string, RepositoryFilterConfiguration>>(yml);
            foreach (KeyValuePair<string, RepositoryFilterConfiguration> item in result)
            {
                item.Value.Name = item.Key;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{file}'. {message}", file, ex.Message);
            throw;
        }
    }
}