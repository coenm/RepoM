namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;

internal class ChangeEventDummyFileSystemWatcher : IFileSystemWatcher
{
    public IFileSystem FileSystem { get; }

    public void Dispose()
    {
    }

    public void BeginInit()
    {
    }

    public void EndInit()
    {
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
    {
        throw new NotImplementedException();
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
    {
        throw new NotImplementedException();
    }

    public void Change(FileSystemEventArgs evt)
    {
        Changed?.Invoke(this, evt);
    }

    public IContainer? Container { get; }
    public bool EnableRaisingEvents { get; set; }
    public string Filter { get; set; }
    public Collection<string> Filters { get; }
    public bool IncludeSubdirectories { get; set; }
    public int InternalBufferSize { get; set; }
    public NotifyFilters NotifyFilter { get; set; }
    public string Path { get; set; }
    public ISite? Site { get; set; }
    public ISynchronizeInvoke? SynchronizingObject { get; set; }
    public event FileSystemEventHandler? Changed;
    public event FileSystemEventHandler? Created;
    public event FileSystemEventHandler? Deleted;
    public event ErrorEventHandler? Error;
    public event RenamedEventHandler? Renamed;
}