namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Api.Common;

public class DefaultDriveEnumerator : IPathProvider
{
    private readonly IAppSettingsService _appsettings;
    private readonly IFileSystem _fileSystem;

    public DefaultDriveEnumerator(IAppSettingsService appsettings, IFileSystem fileSystem)
    {
        _appsettings = appsettings;
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string[] GetPaths()
    {
        // Check if the ReposRootDirectories is valid and use that for searching repos
        if (_appsettings.ReposRootDirectories.Any())
        {
            HashSet<string> paths = new();

            foreach (var reposRootDirectoryPath in _appsettings.ReposRootDirectories)
            {
                DirectoryInfo reposRootDirectory = new(reposRootDirectoryPath);

                if (reposRootDirectory.Exists)
                {
                    _ = paths.Add(reposRootDirectoryPath);
                }
            }

            if (paths.Any())
            {
                return paths.ToArray();
            }
        }

        // By default search all drives for repos
        return _fileSystem.DriveInfo.GetDrives()
                         .Where(d => d.DriveType == DriveType.Fixed)
                         .Select(d => d.RootDirectory.FullName)
                         .ToArray();
    }
}