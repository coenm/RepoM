namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FilesCompareSettingsService : ICompareSettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IEnumerable<IConfigurationRegistration> _registrations;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private Dictionary<string, IRepositoriesComparerConfiguration>? _configuration;
    
    public FilesCompareSettingsService(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        IEnumerable<IConfigurationRegistration> registrations,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registrations = registrations.ToList();
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
    }

    public Dictionary<string, IRepositoriesComparerConfiguration> Configuration => _configuration ??= Load();


    private string GetFileName()
    {
        return _fileSystem.Path.Combine(_appDataPathProvider.AppDataPath, "RepoM.Ordering.yaml");
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> Load()
    {
        try
        {
            return LoadInner();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not read and parse {filename} file. The structure has changed, maybe it is an easy fix by just replacing all '@' signs with the fixed string 'type: ' and you will be good to go. {message}", GetFileName(), e.Message);
            throw;
        }
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> LoadInner()
    {
        var file = GetFileName();

        if (!_fileSystem.File.Exists(file))
        {
            throw new FileNotFoundException("Comparer configuration file not found", file);
        }

        try
        {
            var yml = _fileSystem.File.ReadAllText(file);

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                        {
                            options.AddKeyValueTypeDiscriminator<IRepositoryScorerConfiguration>(
                                "type",
                                _registrations
                                    .Where(reg => typeof(IRepositoryScorerConfiguration).GetTypeInfo().IsAssignableFrom(reg.ConfigurationType.GetTypeInfo()))
                                    .ToDictionary(registration => registration.Tag, x => x.ConfigurationType));

                            options.AddKeyValueTypeDiscriminator<IRepositoriesComparerConfiguration>(
                                "type",
                                _registrations
                                    .Where(reg => typeof(IRepositoriesComparerConfiguration).GetTypeInfo().IsAssignableFrom(reg.ConfigurationType.GetTypeInfo()))
                                    .ToDictionary(registration => registration.Tag, x => x.ConfigurationType));
                        },
                     maxDepth: -1,
                     maxLength: -1)
                .Build();

            return deserializer.Deserialize<Dictionary<string, IRepositoriesComparerConfiguration>>(yml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{file}'. {message}", file, ex.Message);
            throw;
        }
    }
}