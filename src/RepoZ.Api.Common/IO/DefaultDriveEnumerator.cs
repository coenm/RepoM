namespace RepoZ.Api.Common.IO;

using System;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Api.IO;

public class DefaultDriveEnumerator : IPathProvider
{
    private readonly IFileSystem _fileSystem;

    public DefaultDriveEnumerator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string[] GetPaths()
    {
        return _fileSystem.DriveInfo.GetDrives()
                          .Where(d => d.DriveType == System.IO.DriveType.Fixed)
                          .Select(d => d.RootDirectory.FullName)
                          .ToArray();
    }
}