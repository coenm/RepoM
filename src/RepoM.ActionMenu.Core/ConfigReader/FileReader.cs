namespace RepoM.ActionMenu.Core.ConfigReader;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model;

internal class FileReader : IFileReader
{
    private static readonly LoadOptions _loadOptions = new (setEnvVars: false);
    private readonly IFileSystem _fileSystem;
    private readonly IActionMenuDeserializer _deserializer;
    private readonly ILogger _logger;

    public FileReader(IFileSystem fileSystem, IActionMenuDeserializer deserializer, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ActionMenuRoot?> DeserializeRoot(string filename)
    {
        return Deserialize<ActionMenuRoot>(filename);
    }

    public Task<TagsRoot?> DeserializeTagsRoot(string filename)
    {
        return Deserialize<TagsRoot>(filename);
    }

    public Task<ContextRoot?> DeserializeContextRoot(string filename)
    {
        return Deserialize<ContextRoot>(filename);
    }

    public async Task<IDictionary<string, string>?> ReadEnvAsync(string filename)
    {
        if (!_fileSystem.File.Exists(filename))
        {
            return null;
        }

        var content = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
        return Env.LoadContents(content, _loadOptions).ToDictionary();
    }

    private async Task<T?> Deserialize<T>(string filename) where T : ContextRoot
    {
        if (!_fileSystem.File.Exists(filename))
        {
            return null;
        }

        try
        {
            var content = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            return _deserializer.Deserialize<T>(content);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not deserialize {Filename} as {Type}", filename, typeof(T).Name);
            throw;
        }
    }
}