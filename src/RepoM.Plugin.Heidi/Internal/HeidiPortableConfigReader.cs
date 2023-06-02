namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using RepoM.Plugin.Heidi.Internal.Config;

internal class HeidiPortableConfigReader : IHeidiPortableConfigReader
{
    private const string KEYWORD_SERVER = "Servers\\";
    private const string KEYWORD_SPLIT = "<|||>";
    
    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPasswordDecoder _passwordDecoder;

    public HeidiPortableConfigReader(IFileSystem fileSystem, IHeidiPasswordDecoder passwordDecoder)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _passwordDecoder = passwordDecoder ?? throw new ArgumentNullException(nameof(passwordDecoder));
    }

    public async Task<List<HeidiSingleDatabaseConfiguration>> ParseAsync(string filename)
    {
        List<HeidiSingleDatabaseRawConfiguration> parseResult = await ParseConfiguration2Async(filename).ConfigureAwait(false);

        var result = new List<HeidiSingleDatabaseConfiguration>();

        foreach (HeidiSingleDatabaseRawConfiguration item in parseResult)
        {
            result.Add(new HeidiSingleDatabaseConfiguration(item.Key)
                {
                    Comment = item.Comment,
                    Databases = item.Databases.Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray(),
                    Host = item.Host,
                    Password = _passwordDecoder.DecodePassword(item.EncryptedPassword), // can throw
                    WindowsAuth = item.WindowsAuth == 1,
                    User = item.User,
                    Port = int.Parse(item.Port),
                    NetType = item.NetType,
                    Library = item.Library,
                });
        }

        return result;
    }

    internal async Task<List<HeidiSingleDatabaseRawConfiguration>> ParseConfiguration2Async(string filename)
    {
        List<HeidiSingleLineConfiguration> config = await ParseSingleLinesAsync(filename).ConfigureAwait(false);

        return (from @group in config.GroupBy(x => x.Key)
                let lines = @group.ToList()
                select new HeidiSingleDatabaseRawConfiguration
                    {
                        Key = @group.Key,
                        Host = GetStringValue(lines, "Host", string.Empty),
                        User = GetStringValue(lines, "User", string.Empty),
                        EncryptedPassword = GetStringValue(lines, "Password", string.Empty),
                        Port = GetStringValue(lines, "Port", "0"),
                        WindowsAuth = GetIntValue(lines, "WindowsAuth", -1),
                        NetType = GetIntValue(lines, "NetType", -1),
                        Library = GetStringValue(lines, "Library", string.Empty),
                        Comment = GetStringValue(lines, "Comment", string.Empty),
                        Databases = GetStringValue(lines, "Databases", string.Empty),
                    })
            .ToList();
    }

    private static string GetStringValue(IEnumerable<HeidiSingleLineConfiguration> input, string key, string defaultValue = "")
    {
        HeidiSingleLineConfiguration[] foundItems = input.Where(x => key.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase)).ToArray();

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
        HeidiSingleLineConfiguration[] foundItems = input.Where(x => key.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase)).ToArray();

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
    
    internal async Task<List<HeidiSingleLineConfiguration>> ParseSingleLinesAsync(string filename)
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
}