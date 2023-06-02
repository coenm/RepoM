namespace RepoM.Api.Git;

using System;

public class IgnoreRule
{
    private readonly Func<string, bool> _comparer;

    public IgnoreRule(string pattern)
    {
        var wildcardStart = pattern.StartsWith('*');
        var wildcardEnd = pattern.EndsWith('*');

        if (wildcardStart || wildcardEnd)
        {
            if (wildcardStart)
            {
                pattern = pattern[1..];
            }

            if (wildcardEnd)
            {
                pattern = pattern[..^1];
            }

            if (wildcardStart && wildcardEnd)
            {
                _comparer = path => path.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (wildcardStart)
                {
                    _comparer = path => path.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    _comparer = path => path.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        else
        {
            _comparer = path => string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool IsIgnored(string path)
    {
        return _comparer(path);
    }
}