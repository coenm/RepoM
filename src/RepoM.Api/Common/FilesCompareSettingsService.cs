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
        return _fileSystem.Path.Combine(_appDataPathProvider.GetAppDataPath(), "RepoM.Ordering.yaml");
    }

    private Dictionary<string, IRepositoriesComparerConfiguration> Load()
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

            foreach (IConfigurationRegistration instance in _registrations)
            {
                var tag = instance.Tag.TrimStart('!');
                builder.WithTagMapping("!" + tag, instance.ConfigurationType);
            }

            IDeserializer deserializer = builder.Build();

            return deserializer.Deserialize<Dictionary<string, IRepositoriesComparerConfiguration>>(yml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load configuration from file '{file}'. {message}", file, ex.Message);
            throw;
        }
    }
}