namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FilesCompareSettingsService : ICompareSettingsService
{
    private const string FILENAME = "RepoM.Ordering.yaml";
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private Dictionary<string, IRepositoriesComparerConfiguration>? _configuration;
    private readonly IDeserializer _deserializer;

    public FilesCompareSettingsService(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        IEnumerable<IKeyTypeRegistration<IRepositoriesComparerConfiguration>> comparerRegistrations,
        IEnumerable<IKeyTypeRegistration<IRepositoryScorerConfiguration>> scorerRegistrations,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));

        var comparerTypesDictionary = comparerRegistrations.ToDictionary(registration => registration.Tag, registration => registration.ConfigurationType);
        var scorerTypesDictionary = scorerRegistrations.ToDictionary(registration => registration.Tag, registration => registration.ConfigurationType);

        _deserializer = new DeserializerBuilder()
             .WithNamingConvention(HyphenatedNamingConvention.Instance)
             .WithTypeDiscriminatingNodeDeserializer(options =>
                     {
                         options.AddKeyValueTypeDiscriminator<IRepositoryScorerConfiguration>(nameof(IRepositoryScorerConfiguration.Type).ToLower(), scorerTypesDictionary);
                         options.AddKeyValueTypeDiscriminator<IRepositoriesComparerConfiguration>(nameof(IRepositoriesComparerConfiguration.Type).ToLower(), comparerTypesDictionary);
                     },
                 maxDepth: -1,
                 maxLength: -1)
             .Build();
    }

    public Dictionary<string, IRepositoriesComparerConfiguration> Configuration => _configuration ??= Load();
    
    private string GetFileName()
    {
        return _fileSystem.Path.Combine(_appDataPathProvider.AppDataPath, FILENAME);
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> Load()
    {
        try
        {
            return LoadInner();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not read and parse {Filename} file. The structure has changed, maybe it is an easy fix by just replacing all '@' signs with the fixed string 'type: ' and you will be good to go. {Message}", GetFileName(), e.Message);
            throw;
        }
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> LoadInner()
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
                throw new FileNotFoundException("Comparer configuration file not found", file);
            }
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(file);
            return _deserializer.Deserialize<Dictionary<string, IRepositoriesComparerConfiguration>>(yml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{File}'. {Message}", file, ex.Message);
            throw;
        }
    }
}