namespace RepoM.ActionMenu.Core.Model.Context;

using System;
using RepoM.ActionMenu.Interface.Attributes;

internal partial class FileFunctions
{
    /// <summary>
    /// Checks if the specified file path exists on the disk.
    /// </summary>
    /// <param name="path">Absolute path to a file.</param>
    /// <returns>`true` if the specified file path exists on the disk, `false` otherwise.</returns>
    [ActionMenuMember("file_exists")]
    public static bool FileExists(ActionMenuGenerationContext context, string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentOutOfRangeException(nameof(path), "Path cannot be null or empty.");
        }

        return context.FileSystem.File.Exists(path);
    }

    /// <summary>
    /// Checks if the specified directory path exists on the disk.
    /// </summary>
    /// <param name="path">Absolute path to a directory.</param>
    /// <returns>`true` if the specified directory path exists on the disk, `false` otherwise.</returns>
    [ActionMenuMember("dir_exists")]
    public static bool DirectoryExists(ActionMenuGenerationContext context, string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentOutOfRangeException(nameof(path), "Path cannot be null or empty.");
        }

        return context.FileSystem.Directory.Exists(path);
    }
}