namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class FilesCompareSettingsService : ICompareSettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnumerable<IConfigurationRegistration> _registrations;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private Dictionary<string, IRepositoriesComparerConfiguration>? _configuration;


    public FilesCompareSettingsService(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, IEnumerable<IConfigurationRegistration> registrations)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
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
            throw new Exception("File doesn't exist");
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
        catch
        {
            throw;
            /* Our app settings are not critical. For our purposes, we want to ignore IO exceptions */
        }
    }
}