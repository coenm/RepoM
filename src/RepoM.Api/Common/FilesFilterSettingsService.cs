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
        return _fileSystem.Path.Combine(_appDataPathProvider.AppDataPath, FILENAME);
    }

    private Dictionary<string, RepositoryFilterConfiguration> Load()
    {
        var file = GetFileName();

        if (!_fileSystem.File.Exists(file))
        {
            var templateFilename = _fileSystem.Path.Combine(_appDataPathProvider.AppResourcesPath, FILENAME);
            if (_fileSystem.File.Exists(templateFilename))
            {
                try
                {
                    _fileSystem.File.Copy(templateFilename, file);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not copy template file '{TemplateFilename}' to '{File}'", templateFilename, file);
                }
            }

            if (!_fileSystem.File.Exists(file))
            {
                throw new FileNotFoundException("Filtering configuration file not found", file);
            }
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(file);

            DeserializerBuilder builder = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance);

            IDeserializer deserializer = builder.Build();

            Dictionary<string, RepositoryFilterConfiguration>? result = deserializer.Deserialize<Dictionary<string, RepositoryFilterConfiguration>?>(yml);
            if (result == null)
            {
                return new Dictionary<string, RepositoryFilterConfiguration>();
            }

            foreach (KeyValuePair<string, RepositoryFilterConfiguration> item in result)
            {
                item.Value.Name = item.Key;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{File}'. {Message}", file, ex.Message);
            throw;
        }
    }
}