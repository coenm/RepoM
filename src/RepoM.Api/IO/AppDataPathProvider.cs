namespace RepoM.Api.IO;

using System;
using System.IO.Abstractions;
using RepoM.Core.Plugin.Common;

public sealed class AppDataPathProvider : IAppDataPathProvider
{ 
    public AppDataPathProvider(AppDataPathConfig config, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);

        if (!string.IsNullOrWhiteSpace(config.AppSettingsPath))
        {
            AppDataPath = fileSystem.Path.GetFullPath(config.AppSettingsPath);
            return;
        }

        AppDataPath = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoM");
    }

    public string AppDataPath { get; }
}