namespace RepoM.Api.IO;

using System;
using System.IO;
using RepoM.Core.Plugin.Common;

public sealed class AppDataPathProvider : IAppDataPathProvider
{ 
    public AppDataPathProvider(AppDataPathConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!string.IsNullOrWhiteSpace(config.AppSettingsPath))
        {
            AppDataPath = Path.GetFullPath(config.AppSettingsPath);
            return;
        }

        AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoM");
    }

    public string AppDataPath { get; }
}