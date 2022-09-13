namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Api.Common;

public abstract class FileRepositoryStore : IRepositoryStore
{
    private protected readonly IFileSystem FileSystem;

    protected FileRepositoryStore(IFileSystem fileSystem)
    {
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public abstract string GetFileName();

    public IEnumerable<string> Get(string file)
    {
        if (!UseFilePersistence)
        {
            return Array.Empty<string>();
        }

        if (FileSystem.File.Exists(file))
        {
            try
            {
                return FileSystem.File.ReadAllLines(file);
            }
            catch (Exception)
            {
                // swallow for now.
            }
        }

        return Array.Empty<string>();
    }

    public IEnumerable<string> Get()
    {
        var file = GetFileName();
        return Get(file);
    }

    public void Set(IEnumerable<string> paths)
    {
        if (!UseFilePersistence)
        {
            return;
        }

        var file = GetFileName();
        var path = FileSystem.Directory.GetParent(file).FullName;

        if (!FileSystem.Directory.Exists(path))
        {
            FileSystem.Directory.CreateDirectory(path);
        }

        try
        {
            FileSystem.File.WriteAllLines(GetFileName(), paths.ToArray());
        }
        catch (Exception)
        {
            // swallow for now.
        }
    }

    public bool UseFilePersistence { get; set; } = true;
}