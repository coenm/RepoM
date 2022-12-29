namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RepoM.Plugin.Heidi.Internal.Config;

internal class HeidiPortableConfigReader
{
    private readonly IFileSystem _fileSystem;

    public HeidiPortableConfigReader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public async Task<Dictionary<string, RepomHeidiConfig>> ReadConfigsAsync(string filename)
    {
        const string KEYWORD_SERVER = "Servers\\";
        const string KEYWORD_COMMENT = "\\Comment<|||>";
        const string KEYWORD_START = "#REPOM_START#";
        const string KEYWORD_END = "#REPOM_END#";

        var result = new Dictionary<string, RepomHeidiConfig>();
        string[] lines = await _fileSystem.File.ReadAllLinesAsync(filename);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Length < 10)
            {
                continue;
            }

            if (!line.StartsWith(KEYWORD_SERVER))
            {
                continue;
            }

            var indexComment = line.IndexOf(KEYWORD_COMMENT, StringComparison.InvariantCulture);
            if (indexComment < 1)
            {
                continue;
            }

            var indexRepomStart = line.IndexOf(KEYWORD_START, StringComparison.InvariantCulture);
            if (indexRepomStart <= indexComment)
            {
                continue;
            }

            var indexRepomEnd = line.IndexOf(KEYWORD_END, StringComparison.InvariantCulture);
            if (indexRepomEnd <= indexRepomStart)
            {
                continue;
            }

            var key = line[KEYWORD_SERVER.Length..indexComment];
            var json = line.Substring(indexRepomStart + KEYWORD_START.Length, indexRepomEnd - indexRepomStart - KEYWORD_START.Length);

            RepomHeidiConfigV1? config = JsonConvert.DeserializeObject<RepomHeidiConfigV1>(json);
            if (config != null)
            {
                config.HeidiKey = key;
                result.Add(key, config);
            }
        }

        return result;
    }
}