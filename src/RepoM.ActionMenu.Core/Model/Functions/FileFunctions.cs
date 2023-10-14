namespace RepoM.ActionMenu.Core.Model.Functions;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using Scriban.Parsing;
using Scriban.Syntax;

[ActionMenuModule("File")]
internal partial class FileFunctions : ScribanModuleWithFunctions
{
    private readonly IFileSystem _fileSystem;

    public FileFunctions(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        RegisterFunctions();
    }


    /// <summary>
    /// Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.
    /// </summary>
    /// <param name="rootPath">The root folder.</param>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.</param>
    /// <returns>Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.</returns>
    /// <example>first</example>
    /// <example>
    /// second
    /// <code>
    /// find_files 'C:\Users\coenm\RepoM\src' '*.sln'
    /// </code>
    /// <para>
    /// xxx</para>
    /// sdf
    /// 
    /// ```scribanhtml
    /// find_files 'C:\Users\coenm\RepoM\src' '*.csproj'
    /// ```
    /// ```html
    /// [1, 2, 3, 4]
    /// ```
    /// </example>
    [ActionMenuMember("find_files")]
    public static string[] FindFiles(ActionMenuGenerationContext context, SourceSpan span, string rootPath, string searchPattern)
    {
        return FindFilesUsingInterface(context, span, rootPath, searchPattern);
    }

    /// <inheritdoc cref="FindFiles"/>
    [ActionMenuMember("find_files_interface")]
    public static string[] FindFilesUsingInterface(IMenuContext context, SourceSpan span, string rootPath, string searchPattern)
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

    private static IEnumerable<string> GetFileEnumerator(IFileSystem fileSystem, string path, string searchPattern)
    {
        // prefer EnumerateFileSystemInfos() over EnumerateFiles() to include packaged folders like
        // .app or .xcodeproj on macOS
        IDirectoryInfo directory = fileSystem.DirectoryInfo.New(path);

        return directory
            .EnumerateFileSystemInfos(searchPattern, SearchOption.AllDirectories)
            .Select(f => f.FullName)
            .Where(f => !f.StartsWith('.'));
    }
}