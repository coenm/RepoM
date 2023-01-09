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

internal class ExtractRepositoryFromHeidi
{
    private const string KEYWORD_NEWLINE = "<{{{><}}}>";

    public bool TryExtract(HeidiSingleDatabaseConfiguration config, [NotNullWhen(true)] out RepoHeidi? i)
    {
        i = null;
        // return false;

        ReadOnlySpan<char> comment = config.Comment.Replace(KEYWORD_NEWLINE, " ").AsSpan();
        if (string.IsNullOrWhiteSpace(config.Comment))
        {
            return false;
        }
        
        var repos = new List<string>(1);

        var index = comment.IndexOf('#');
        if (index > 0)
        {
            index = comment.IndexOf(" #");
        }

        while (index > -1)
        {
            comment = comment[(index + 1) ..];

            if (comment.StartsWith("#repo:", StringComparison.CurrentCultureIgnoreCase))
            {
                // #repo: <one or more>

                // #repo:"abc def"
                // #repo:abc

                comment = comment["#repo:".Length..];

                if (comment[0].Equals('"'))
                {
                    // yes
                    comment = comment[1..];
                    var endIndex = comment.IndexOf('"');
                    if (endIndex > 0)
                    {
                        var repo = comment[0..endIndex].ToString();
                        if (!string.IsNullOrWhiteSpace(repo))
                        {
                            repos.Add(repo);

                        }
                        // foreach (var c in repo)
                        // {
                        //     //a-z, A-Z, 0-9, \s ._-
                        //
                        //     
                        // }
                    }
                }
                else
                {
                    char[] allowedChars = new[] { '.', '-', '_'/*, ' '*/, };

                    var k = 0;
                    bool stop = false;
                    while (k < comment.Length && !stop )
                    {
                        k++;
                        if (comment[k] is >= 'a' and <= 'z')
                        {
                            continue;
                        }
                        if (comment[k] is >= 'A' and <= 'Z')
                        {
                            continue;
                        }
                        if (allowedChars.Contains(comment[k]))
                        {
                            continue;
                            
                        }

                        // yes
                        stop = true;
                    }

                    var repo = comment[..k].ToString();
                    if (!string.IsNullOrWhiteSpace(repo))
                    {
                        repos.Add(repo);
                    }

                    comment = comment[k..];
                }
            }
            else if (comment.StartsWith("order:", StringComparison.InvariantCultureIgnoreCase))
            {
                comment = comment[("order:".Length + 1)..];


            }
            else if (comment.StartsWith("name:", StringComparison.CurrentCultureIgnoreCase))
            {
                comment = comment[("name:".Length + 1)..];

            }
            else
            {
                // tag
                // stop until space or eof
            }
            
            // #order: <single>
            // #name: <single>
            // #xx <tag, zero or more>


            index = comment.IndexOf(" #");
        }
        
        i = new RepoHeidi
            {
                Order = 12,
                HeidiKey = config.Key,
                RepositoryName = repos.FirstOrDefault() ?? string.Empty,
                Tags = Array.Empty<string>(),
            };
        return true;
    }
}

internal class HeidiPortableConfigReader : IHeidiPortableConfigReader
{
    private const string KEYWORD_SERVER = "Servers\\";
    private const string KEYWORD_SPLIT = "<|||>";


    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IHeidiPasswordDecoder _passwordDecoder;

    public HeidiPortableConfigReader(IFileSystem fileSystem, ILogger logger, IHeidiPasswordDecoder passwordDecoder)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordDecoder = passwordDecoder ?? throw new ArgumentNullException(nameof(passwordDecoder));
    }

    public async Task<List<HeidiSingleDatabaseConfiguration>> Parse(string filename)
    {
        var parseResult = await ParseConfiguration2Async(filename).ConfigureAwait(false);

        var result = new List<HeidiSingleDatabaseConfiguration>();

        foreach (var item in parseResult)
        {
            result.Add(new HeidiSingleDatabaseConfiguration
                {
                    Comment = item.Comment,
                    Databases = item.Databases.Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray(),
                    Host = item.Host,
                    Key = item.Key,
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

    public async Task<List<HeidiSingleDatabaseRawConfiguration>> ParseConfiguration2Async(string filename)
    {
        var config = await ParseConfigurationAsync(filename).ConfigureAwait(false);

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