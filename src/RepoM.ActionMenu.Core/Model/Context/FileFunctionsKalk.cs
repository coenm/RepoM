namespace RepoM.ActionMenu.Core.Model.Context;

using System;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;

internal partial class FileFunctions
{
    /// <summary>
    /// Checks if the specified file path exists on the disk.
    /// </summary>
    /// <param name="path">Absolute path to a file.</param>
    /// <returns>`true` if the specified file path exists on the disk, `false` otherwise.</returns>
    /// <example>
    /// <usage/>
    /// Check if file exists
    /// <code>
    /// exists = file.file_exists('C:\Project\my-solution.sln');
    /// </code>
    /// <repository-action-sample/>
    /// TODO: this content is not correct, change filename
    /// <code-file language='yaml' filename='file.find_files.actionmenu.yaml' />
    /// </example>
    [ActionMenuMember("file_exists")]
    public static bool FileExists(ActionMenuGenerationContext context, string path)
    {
        return FileExists(context as IMenuContext, path);
    }

    internal static bool FileExists(IMenuContext context, string path)
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
    /// <example>
    /// <usage/>
    /// Check if file exists
    /// <code>
    /// exists = file.dir_exists('C:\Project\');
    /// exists = file.dir_exists('C:\Project');
    /// exists = file.dir_exists('C:/Project/');
    /// </code>
    /// <repository-action-sample/>
    /// TODO: this content is not correct, change filename
    /// <code-file language='yaml' filename='file.find_files.actionmenu.yaml' />
    /// </example>
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