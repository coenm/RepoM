namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Plugin.Heidi.Internal.Config;

internal class HeidiPortableConfigReader : IHeidiPortableConfigReader
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public HeidiPortableConfigReader(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var indexStart = line.IndexOf(KEYWORD_START, StringComparison.InvariantCulture);
            if (indexStart <= indexComment)
            {
                continue;
            }

            var indexEnd = line.IndexOf(KEYWORD_END, StringComparison.InvariantCulture);
            if (indexEnd <= indexStart)
            {
                continue;
            }

            try
            {
                var key = line[KEYWORD_SERVER.Length..indexComment];
                var json = line.Substring(indexStart + KEYWORD_START.Length, indexEnd - indexStart - KEYWORD_START.Length);

                RepomHeidiConfig? config = JsonConvert.DeserializeObject<RepomHeidiConfig>(json);
                if (config != null)
                {
                    config.HeidiKey = key;
                    result.Add(key, config);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Could not deserialize heidi config {message}", e.Message);
            }
        }

        return result;
    }
}