namespace RepoM.Core.Repositories.Tests.Watching;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;

internal sealed class DummyFileSystemWatcher : IFileSystemWatcher
{
    public void Dispose()
    {
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

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, TimeSpan timeout)
    {
        throw new NotSupportedException();
    }

    public void SimulateCreated(string path, string filename)
    {
        Created?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, path, filename));
    }

    public void SimulateChanged(string path, string filename)
    {
        Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, path, filename));
    }

    public void SimulateDeleted(string path, string filename)
    {
        Deleted?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, path, filename));
    }

    public void SimulateRenamed(string path, string newFilename, string oldFilename)
    {
        Renamed?.Invoke(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, path, newFilename, oldFilename));
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

    public event FileSystemEventHandler? Changed = delegate { };

    public event FileSystemEventHandler? Created = delegate { };

    public event FileSystemEventHandler? Deleted = delegate { };

    public event ErrorEventHandler? Error = delegate { };

    public event RenamedEventHandler? Renamed = delegate { };
}
