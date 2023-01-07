namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Plugin.Heidi.Internal.Config;

internal class HeidiPortableConfigReader : IHeidiPortableConfigReader
{
    private const string KEYWORD_SERVER = "Servers\\";
    private const string KEYWORD_SPLIT = "<|||>";
    private const string KEYWORD_NEWLINE = "<{{{><}}}>";

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public HeidiPortableConfigReader(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<HeidiSingleDatabaseConfiguration>> ParseConfiguration2Async(string filename)
    {
        var config = await ParseConfigurationAsync(filename).ConfigureAwait(false);

        var result = new List<HeidiSingleDatabaseConfiguration>();
        
        foreach (IGrouping<string, HeidiSingleLineConfiguration> group in config.GroupBy(x => x.Key))
        {
            var lines = group.ToList();

            result.Add(new HeidiSingleDatabaseConfiguration
                {
                    Key = group.Key,
                    Host = GetStringValue(lines, "Host", string.Empty),
                    User = GetStringValue(lines, "User", string.Empty),
                    EncryptedPassword = GetStringValue(lines, "Password", string.Empty),
                    Port = GetStringValue(lines, "Port", string.Empty),
                    WindowsAuth = GetIntValue(lines, "WindowsAuth", -1),
                    NetType = GetIntValue(lines, "NetType", -1),
                    Library = GetStringValue(lines, "Library", string.Empty),
                    Comment = GetStringValue(lines, "Comment", string.Empty),
                    Databases = GetStringValue(lines, "Databases", string.Empty),
                });
        }

        return result;
    }

    private static string GetStringValue(IEnumerable<HeidiSingleLineConfiguration> input, string key, string defaultValue = "")
    {
        var foundItems = input.Where(x => key.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase)).ToArray();

        if (foundItems.Length == 0)
        {
            return defaultValue;
        }

        if (!"1".Equals(foundItems[0].ContentType, StringComparison.InvariantCulture))
        {
            return defaultValue;
        }

        return foundItems[0].Content ?? defaultValue;
    }

    private static int GetIntValue(IEnumerable<HeidiSingleLineConfiguration> input, string key, int defaultValue = -1)
    {
        var foundItems = input.Where(x => key.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase)).ToArray();

        if (foundItems.Length == 0)
        {
            return defaultValue;
        }

        if (!"3".Equals(foundItems[0].ContentType, StringComparison.InvariantCulture))
        {
            return defaultValue;
        }

        var content = foundItems[0].Content ?? string.Empty;

        if (!int.TryParse(content, out var result))
        {
            return defaultValue;
        }

        return result;
    }
    
    public async Task<List<HeidiSingleLineConfiguration>> ParseConfigurationAsync(string filename)
    {
        string[] lines = await _fileSystem.File.ReadAllLinesAsync(filename).ConfigureAwait(false);
        var result = new List<HeidiSingleLineConfiguration>();
        foreach (var line in lines)
        {
            if (TrySplitLine(line, out HeidiSingleLineConfiguration? singleLineConfig))
            {
                result.Add(singleLineConfig.Value);
            }
        }

        return result;
    }

   private static bool TrySplitLine(in string line, [NotNullWhen(true)] out HeidiSingleLineConfiguration? heidiConfig)
    {
        heidiConfig = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (line.Length < 10)
        {
            return false;
        }

        if (!line.StartsWith(KEYWORD_SERVER))
        {
            return false;
        }

        ReadOnlySpan<char> lineSpan = line.AsSpan(KEYWORD_SERVER.Length);
        var indexComment = lineSpan.IndexOf(KEYWORD_SPLIT, StringComparison.InvariantCulture);
        if (indexComment < 1)
        {
            return false;
        }

        var indexSeparatorKeyType = lineSpan[..indexComment].LastIndexOf('\\');
        if (indexSeparatorKeyType < 1)
        {
            return false;
        }

        var indexSplit2 = lineSpan[(indexComment + KEYWORD_SPLIT.Length)..].IndexOf(KEYWORD_SPLIT, StringComparison.InvariantCulture);
        if (indexSplit2 < 1)
        {
            return false;
        }
        
        var key = lineSpan[..indexSeparatorKeyType].ToString();
        var type = lineSpan[(indexSeparatorKeyType+1)..indexComment].ToString();
        var contentType = lineSpan[(indexComment + KEYWORD_SPLIT.Length)..(indexComment + KEYWORD_SPLIT.Length + indexSplit2)].ToString();
        var content = lineSpan[(indexComment + KEYWORD_SPLIT.Length + indexSplit2 + KEYWORD_SPLIT.Length)..].ToString();

        heidiConfig = new HeidiSingleLineConfiguration
            {
                Key = key,
                Type = type,
                ContentType = contentType,
                Content = content,
            };

        return true;
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