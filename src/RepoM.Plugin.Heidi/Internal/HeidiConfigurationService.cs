namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

internal sealed class HeidiConfigurationService : IDisposable
{
    const string FILENAME = "C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable\\";
    const string FILENAME2 = "portable_settings.txt";

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly HeidiPortableConfigReader _reader;
    private IFileSystemWatcher? _fileWatcher;

    public HeidiConfigurationService(
        ILogger logger,
        IFileSystem fileSystem,
        HeidiPortableConfigReader reader)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Task InitializeAsync()
    {
       // if (_fileSystem.File.Exists(FILENAME))
        {
            _fileWatcher = _fileSystem.FileSystemWatcher.New(FILENAME, FILENAME2);
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileWatcher.EnableRaisingEvents = true;
            _fileWatcher.Changed += XOnChanged;
        }

       
       return Task.CompletedTask;
    }

    private void XOnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File changed '{name}' '{type}' '{fullPath}'", e.Name, e.ChangeType, e.FullPath);
    }
    
    public void Dispose()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.Changed -= XOnChanged;
        }
       
        _fileWatcher?.Dispose();
    }
}