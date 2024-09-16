namespace RepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using Scriban.Parsing;
using Scriban.Syntax;

/// <summary>
/// Provides file related action menu functions and variables accessible through `file`.
/// </summary>
[ActionMenuContext("file")]
internal partial class FileFunctions : ScribanModuleWithFunctions
{
    private static readonly EnumerationOptions _findFilesOptions = new()
    {
        RecurseSubdirectories = true,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Device | FileAttributes.Directory,
        IgnoreInaccessible = true,
        MatchType = MatchType.Simple,
        ReturnSpecialDirectories = false,
    };

    public FileFunctions()
    {
        RegisterFunctions();
    }

    /// <summary>
    /// Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.
    /// </summary>
    /// <param name="context">The scriban context.</param>
    /// <param name="span">The scriban source span.</param>
    /// <param name="rootPath">The root folder.</param>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.</param>
    /// <returns>Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.</returns>
    /// <example>
    /// <usage/>
    /// Locate all solution files in the given directory.
    /// <code>
    /// solution_files = file.find_files('C:\Project\', '*.sln');
    /// </code>
    /// <result/>
    /// As a result, the variable `solution_files` is an enumerable of strings, for example:
    /// <code-file language='yaml' filename='file.find_files.verified.yaml' />
    /// <repository-action-sample/>
    /// <snippet name='find_files@actionmenu01' mode='snippet' />
    /// </example>
    [ActionMenuContextMember("find_files")]
    public static string[] FindFiles(ActionMenuGenerationContext /*IMenuContext*/ context, SourceSpan span, string rootPath, string searchPattern)
    {
        return FindFilesInner(context, span, rootPath, searchPattern);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string[] FindFilesInner(IMenuContext context, SourceSpan span, string rootPath, string searchPattern)
    {
        try
        {
            return GetFileEnumerator(context.FileSystem, rootPath, searchPattern).ToArray();
        }
        catch (Exception e)
        {
            throw new ScriptRuntimeException(span, "Could not get files.", e);
        }
    }

    /// <summary>
    /// Checks if the specified file path exists on the disk.
    /// </summary>
    /// <param name="context">The scriban context.</param>
    /// <param name="path">Absolute path to a file.</param>
    /// <returns>`true` if the specified file path exists on the disk, `false` otherwise.</returns>
    /// <example>
    /// <usage/>
    /// Check if file exists
    /// <code>
    /// exists = file.file_exists('C:\Project\my-solution.sln');
    /// </code>
    /// <repository-action-sample/>
    /// <snippet name='file_exists@actionmenu01' mode='snippet' />
    /// </example>
    [ActionMenuContextMember("file_exists")]
    public static bool FileExists(ActionMenuGenerationContext context, string path)
    {
        return FileExistsInner(context as IMenuContext, path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool FileExistsInner(IMenuContext context, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return context.FileSystem.File.Exists(path);
    }

    /// <summary>
    /// Checks if the specified directory path exists on the disk.
    /// </summary>
    /// <param name="context">The scriban context.</param>
    /// <param name="path">Absolute path to a directory.</param>
    /// <returns>`true` if the specified directory path exists on the disk, `false` otherwise.</returns>
    /// <example>
    /// <usage/>
    /// Check if directory exists
    /// <code>
    /// exists = file.dir_exists('C:\Project\');
    /// exists = file.dir_exists('C:\Project');
    /// exists = file.dir_exists('C:/Project/');
    /// </code>
    /// <repository-action-sample/>
    /// <snippet name='dir_exists@actionmenu01' mode='snippet' />
    /// </example>
    [ActionMenuContextMember("dir_exists")]
    public static bool DirectoryExists(ActionMenuGenerationContext context, string path)
    {
        return DirectoryExistsInner(context as IMenuContext, path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool DirectoryExistsInner(IMenuContext context, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return context.FileSystem.Directory.Exists(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<string> GetFileEnumerator(IFileSystem fileSystem, string path, string searchPattern)
    {
        return fileSystem.Directory.EnumerateFileSystemEntries(path, searchPattern, _findFilesOptions);
    }
}