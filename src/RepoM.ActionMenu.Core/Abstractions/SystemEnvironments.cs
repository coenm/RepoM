namespace RepoM.ActionMenu.Core.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;

internal sealed class SystemEnvironments : IEnvironment
{
    private SystemEnvironments()
    {
    }

    public static SystemEnvironments Instance { get; } = new ();

    public Dictionary<string, string> GetEnvironmentVariables()
    {
        var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
        {
            if (item.Key is not string key || string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (item.Value is not string value)
            {
                continue;
            }

            env.Add(key.Trim(), value);
        }

        return env;
    }
}