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
    private const string FILENAME = "RepoM.Filtering.yaml";
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private Dictionary<string, RepositoryFilterConfiguration>? _configuration;
    private readonly string _filename;
    
    public FilesFilterSettingsService(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(appDataPathProvider);
        _filename = _fileSystem.Path.Combine(appDataPathProvider.AppDataPath, FILENAME);
    }

    public Dictionary<string, RepositoryFilterConfiguration> Configuration => _configuration ??= Load();

    private Dictionary<string, RepositoryFilterConfiguration> Load()
    {
        if (!_fileSystem.File.Exists(_filename))
        {
            throw new FileNotFoundException("Filtering configuration file not found", _filename);
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(_filename);

            DeserializerBuilder builder = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance);

            IDeserializer deserializer = builder.Build();

            Dictionary<string, RepositoryFilterConfiguration>? result = deserializer.Deserialize<Dictionary<string, RepositoryFilterConfiguration>?>(yml);
            if (result == null)
            {
                return [];
            }

            foreach (KeyValuePair<string, RepositoryFilterConfiguration> item in result)
            {
                item.Value.Name = item.Key;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{File}'. {Message}", _filename, ex.Message);
            throw;
        }
    }
}