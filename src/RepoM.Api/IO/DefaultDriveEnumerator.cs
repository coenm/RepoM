namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;

public class DefaultDriveEnumerator : IPathProvider
{
    private readonly IAppSettingsService _appSettings;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public DefaultDriveEnumerator(IAppSettingsService appSettings, IFileSystem fileSystem, ILogger logger)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string[] GetPaths()
    {
        _logger.LogDebug("Called {Method}", nameof(GetPaths));
        var paths = GetPreconfiguredPaths();

        if (paths.Length != 0)
        {
            return paths;
        }

        return _fileSystem.DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Fixed)
            .Select(d => d.RootDirectory.FullName)
            .ToArray();
    }

    private string[] GetPreconfiguredPaths()
    {
        List<string> directories = _appSettings.ReposRootDirectories;

        if (directories.Count == 0)
        {
            return [];
        }

        HashSet<string> paths = new(directories.Count);

        foreach (var path in directories)
        {
            try
            {
                IDirectoryInfo reposRootDirectory = _fileSystem.DirectoryInfo.New(path);

                if (reposRootDirectory.Exists)
                {
                    _ = paths.Add(path);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Configuration error. Path '{Path}' does not exist. {Message}", path, e.Message);
            }
        }

        if (paths.Count == 0)
        {
            return [];
        }

        return [.. paths, ];
    }
}