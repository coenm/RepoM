namespace RepoM.Api.Favorite;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Repositories.Favorite;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public sealed class FavoriteYamlStore : IFavoriteStore
{
    private readonly string _filePath;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public FavoriteYamlStore(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(appDataPathProvider);
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _filePath = _fileSystem.Path.Combine(appDataPathProvider.AppDataPath, "favorites.yaml");

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public IReadOnlyList<string> Load()
    {
        try
        {
            if (!_fileSystem.File.Exists(_filePath))
            {
                return [];
            }

            var yaml = _fileSystem.File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(yaml))
            {
                return [];
            }

            var result = _deserializer.Deserialize<List<string>>(yaml);
            return result ?? [];
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not load favorites from {FilePath}.", _filePath);
            return [];
        }
    }

    public void Save(IReadOnlyList<string> favorites)
    {
        try
        {
            var directory = _fileSystem.Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
            {
                _fileSystem.Directory.CreateDirectory(directory);
            }

            var yaml = _serializer.Serialize(favorites);
            _fileSystem.File.WriteAllText(_filePath, yaml);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not save favorites to {FilePath}.", _filePath);
        }
    }
}
