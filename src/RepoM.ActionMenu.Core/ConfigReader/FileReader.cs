namespace RepoM.ActionMenu.Core.ConfigReader;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using DotNetEnv;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model;

internal class FileReader : IFileReader
{
    private static readonly LoadOptions _loadOptions = new (setEnvVars: false);
    private readonly IFileSystem _fs;
    private readonly IActionMenuDeserializer _deserializer;

    public FileReader(IFileSystem fileSystem, IActionMenuDeserializer deserializer)
    {
        _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
    }

    public async Task<Root?> DeserializeRoot(string filename)
    {
        if (_fs.File.Exists(filename))
        {
            var content = await _fs.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            return _deserializer.DeserializeRoot(content);
        }

        return null;
    }

    public async Task<ContextRoot?> DeserializeContextRoot(string filename)
    {
        if (_fs.File.Exists(filename))
        {
            var content = await _fs.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            return _deserializer.DeserializeContextRoot(content);
        }

        return null;
    }

    public async Task<IDictionary<string, string>?> ReadEnvAsync(string filename)
    {
        if (_fs.File.Exists(filename))
        {
            var content = await _fs.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            return Env.LoadContents(content, _loadOptions).ToDictionary();
        }

        return null;
    }
}