namespace Grr.App.Messages;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
public abstract class FileMessage : DirectoryMessage
{
    protected FileMessage(RepositoryFilterOptions filter, IFileSystem fileSystem)
        : base(filter, fileSystem)
    {
    }

    protected override void ExecuteExistingDirectory(string directory)
    {
        string[] items;

        try
        {
            items = FindItems(directory, Filter).ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred:\n" + ex.ToString());
            return;
        }

        if (items.Length == 0)
        {
            System.Console.WriteLine($"No files found.\n  Directory:\t{directory}\n  Filter:\t{Filter.FileFilter}");
            return;
        }

        ExecuteFound(items);
    }

    protected virtual IEnumerable<string> FindItems(string directory, RepositoryFilterOptions filter)
    {
        SearchOption searchOption = Filter.RecursiveFileFilter
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        return FileSystem.Directory.GetFiles(directory, filter.FileFilter, searchOption)
                         .OrderBy(i => i);
    }

    protected abstract void ExecuteFound(string[] files);
}