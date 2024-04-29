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
    private Dictionary<string, IRepositoriesComparerConfiguration>? _configuration;
    private readonly IDeserializer _deserializer;
    private readonly string _filename;

    public FilesCompareSettingsService(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        IEnumerable<IKeyTypeRegistration<IRepositoriesComparerConfiguration>> comparerRegistrations,
        IEnumerable<IKeyTypeRegistration<IRepositoryScorerConfiguration>> scorerRegistrations,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ArgumentNullException.ThrowIfNull(appDataPathProvider);
        _filename = _fileSystem.Path.Combine(appDataPathProvider.AppDataPath, FILENAME);

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
    
    private Dictionary<string, IRepositoriesComparerConfiguration> Load()
    {
        try
        {
            return LoadInner();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not read and parse {Filename} file. The structure has changed, maybe it is an easy fix by just replacing all '@' signs with the fixed string 'type: ' and you will be good to go. {Message}", _filename, e.Message);
            throw;
        }
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> LoadInner()
    {
        if (!_fileSystem.File.Exists(_filename))
        {
            throw new FileNotFoundException("Comparer configuration file not found", _filename);
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(_filename);
            return _deserializer.Deserialize<Dictionary<string, IRepositoriesComparerConfiguration>>(yml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{File}'. {Message}", _filename, ex.Message);
            throw;
        }
    }
}