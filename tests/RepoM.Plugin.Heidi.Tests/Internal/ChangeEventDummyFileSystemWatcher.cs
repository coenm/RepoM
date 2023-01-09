namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;

internal sealed class ChangeEventDummyFileSystemWatcher : IFileSystemWatcher
{
    public void Dispose()
    {
        // Method intentionally left empty.
    }

    public void BeginInit()
    {
        throw new NotSupportedException();
    }

    public void EndInit()
    {
        throw new NotSupportedException();
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
    {
        throw new NotSupportedException();
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
    {
        throw new NotSupportedException();
    }

    public void Change(string path, string filename)
    {
        Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, path, filename));
    }
    
    public IFileSystem FileSystem => throw new NotSupportedException();

    public IContainer? Container => throw new NotSupportedException();

    public bool EnableRaisingEvents { get; set; }

    public string Filter { get; set; } = string.Empty;

    public Collection<string> Filters => throw new NotSupportedException();

    public bool IncludeSubdirectories { get; set; }

    public int InternalBufferSize { get; set; }

    public NotifyFilters NotifyFilter { get; set; }

    public string Path { get; set; } = string.Empty;

    public ISite? Site { get; set; }

    public ISynchronizeInvoke? SynchronizingObject { get; set; }

    public event FileSystemEventHandler? Changed;

    public event FileSystemEventHandler? Created;

    public event FileSystemEventHandler? Deleted;

    public event ErrorEventHandler? Error;

    public event RenamedEventHandler? Renamed;
}