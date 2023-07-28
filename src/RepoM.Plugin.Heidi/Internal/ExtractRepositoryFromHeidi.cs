namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RepoM.Plugin.Heidi.Internal.Config;

internal class ExtractRepositoryFromHeidi : IHeidiRepositoryExtractor
{
    private const string HASH_TAG_REPO = "#repo:";
    private const string HASH_TAG_ORDER = "#order:";
    private const string HASH_TAG_NAME = "#name:";
    private const string KEYWORD_NEWLINE = "<{{{><}}}>";
    private const StringComparison COMPARISON = StringComparison.InvariantCultureIgnoreCase;

    private ExtractRepositoryFromHeidi()
    {
    }

    public static ExtractRepositoryFromHeidi Instance { get; } = new();

    public bool TryExtract(HeidiSingleDatabaseConfiguration config, [NotNullWhen(true)] out RepoHeidi? output)
    {
        output = null;

        if (string.IsNullOrWhiteSpace(config.Comment))
        {
            return false;
        }

        ReadOnlySpan<char> comment = (" " + config.Comment).Replace(KEYWORD_NEWLINE, " ").AsSpan();

        var repos = new List<string>(1);
        var names = new List<string>(1);
        var tags = new List<string>();
        var orders = new List<int>(1);

        var index = comment.IndexOf(" #");

        while (index > -1)
        {
            comment = comment[(index + 1) ..];

            if (comment.StartsWith(HASH_TAG_REPO, COMPARISON))
            {
                comment = comment[HASH_TAG_REPO.Length..];

                if (comment[0].Equals('"'))
                {
                    comment = comment[1..];
                    var endIndex = comment.IndexOf('"');
                    if (endIndex > 0)
                    {
                        var repo = comment[..endIndex].ToString();
                        if (!string.IsNullOrWhiteSpace(repo))
                        {
                            repos.Add(repo.Trim());
                        }
                    }
                }
                else
                {
                    int k = FindEndCharIndex(comment);
                    if (k > 0 && (k == comment.Length || comment[k] == ' '))
                    {
                        repos.Add(comment[..k].ToString());
                    }

                    comment = comment[k..];
                }
            }
            else if (comment.StartsWith(HASH_TAG_ORDER, COMPARISON))
            {
                comment = comment[HASH_TAG_ORDER.Length..];

                var k = 0;
                var stop = false;
                while (k < comment.Length && !stop)
                {
                    if (comment[k] is >= '0' and <= '9')
                    {
                        k++;
                        continue;
                    }

                    stop = true;
                }

                if (k > 0 && int.TryParse(comment[..k], out var orderInt))
                {
                    if (k == comment.Length || comment[k] == ' ')
                    {
                        orders.Add(orderInt);
                    }
                }

                comment = comment[k..];
            }
            else if (comment.StartsWith(HASH_TAG_NAME, COMPARISON))
            {
                comment = comment[HASH_TAG_NAME.Length..];

                if (comment[0].Equals('"'))
                {
                    comment = comment[1..];
                    var endIndex = comment.IndexOf('"');
                    if (endIndex > 0)
                    {
                        var name = comment[..endIndex].ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            names.Add(name.Trim());
                        }
                    }
                }
                else
                {
                    int k = FindEndCharIndex(comment);
                    if (k > 0 && (k == comment.Length || comment[k] == ' '))
                    {
                        names.Add(comment[..k].ToString());
                    }

                    comment = comment[k..];
                }
            }
            else
            {
                comment = comment["#".Length..];

                int k = FindEndCharIndex(comment);
                if (k > 0 && (k == comment.Length || comment[k] == ' '))
                {
                    tags.Add(comment[..k].ToString());
                }

                comment = comment[k..];
            }

            index = comment.IndexOf(" #");
        }

        if (repos.Count == 0)
        {
            return false;
        }

        output = new RepoHeidi(config.Key)
            {
                Order = orders.FirstOrDefault(int.MaxValue),
                Repository = repos[0],
                Name = names.FirstOrDefault() ?? string.Empty,
                Tags = tags.Distinct().ToArray(),
            };
        return true;
    }

    private static int FindEndCharIndex(ReadOnlySpan<char> comment)
    {
        char[] allowedChars = { '.', '-', '_', };

        var k = 0;
        var stop = false;
        while (k < comment.Length && !stop)
        {
            if (IsValidChar(comment[k]))
            {
                k++;
                continue;
            }

            if (allowedChars.Contains(comment[k]))
            {
                k++;
                continue;
            }

            stop = true;
        }

        return k;
    }

    private static bool IsValidChar(char c)
    {
        switch (c)
        {
            case >= 'a' and <= 'z':
            case >= 'A' and <= 'Z':
            case >= '0' and <= '9':
                return true;
            default:
                return false;
        }
    }
}