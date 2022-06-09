namespace Grr.Messages;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RepoZ.Ipc;

[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
public class ListRepositoryFilesMessage : FileMessage
{
    public ListRepositoryFilesMessage(RepositoryFilterOptions filter, IFileSystem fileSystem)
        : base(filter, fileSystem)
    {
    }

    protected override void ExecuteFound(string[] files)
    {
        foreach (var file in files)
        {
            System.Console.WriteLine(file);
        }
    }

    protected override IEnumerable<string> FindItems(string directory, RepositoryFilterOptions filter)
    {
        SearchOption searchOption = Filter.RecursiveFileFilter
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        if (filter.FileFilter != null)
        {
            // todo Fix IFileSystem
            return /*FileSystem.*/Directory.GetFileSystemEntries(directory, filter.FileFilter, searchOption)
                                           .OrderBy(i => i);
        }

        return Enumerable.Empty<string>();
        
    }

    public override bool ShouldWriteRepositories(Repository[] repositories)
    {
        return false;
    }
}