namespace RepoM.App.Startup;

using System;
using System.Collections.Generic;

internal sealed record StartupArguments(string? PluginPath, string[] ConfigurationArguments)
{
    public static StartupArguments Parse(string[] args)
    {
        string? pluginPath = null;
        var configuration = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            string argument = args[i];
            if (argument is "--demo" or "--worktree-preview" or "--preview-worktree")
            {
                throw new ArgumentException($"'{argument}' has been replaced by --startup-plugin <dll-path>.");
            }

            if (argument != "--startup-plugin" && !argument.StartsWith("--startup-plugin=", StringComparison.Ordinal))
            {
                configuration.Add(argument);
                continue;
            }

            if (pluginPath != null)
            {
                throw new ArgumentException("Only one --startup-plugin can be supplied.");
            }

            if (argument == "--startup-plugin")
            {
                if (++i == args.Length || args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException("--startup-plugin requires a DLL path.");
                }

                pluginPath = args[i];
            }
            else
            {
                pluginPath = argument["--startup-plugin=".Length..];
            }

            if (string.IsNullOrWhiteSpace(pluginPath))
            {
                throw new ArgumentException("--startup-plugin requires a DLL path.");
            }
        }

        return new StartupArguments(pluginPath, configuration.ToArray());
    }
}
