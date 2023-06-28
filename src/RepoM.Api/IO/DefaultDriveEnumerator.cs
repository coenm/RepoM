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
        var paths = GetPreconfiguredPaths();

        if (paths.Any())
        {
            return paths;
        }

        var x = _fileSystem.DriveInfo.GetDrives().ToArray();
        return _fileSystem.DriveInfo.GetDrives()
                         .Where(d => d.DriveType == DriveType.Fixed)
                         .Select(d => d.RootDirectory.FullName)
                         .ToArray();
    }

    private string[] GetPreconfiguredPaths()
    {
        List<string> directories = _appSettings.ReposRootDirectories;

        if (!directories.Any())
        {
            return Array.Empty<string>();
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
                _logger.LogWarning("Configuration error. Path '{path}' does not exist. {message}", path, e.Message);
            }
        }

        if (!paths.Any())
        {
            return Array.Empty<string>();
        }

        return paths.ToArray();
    }
}