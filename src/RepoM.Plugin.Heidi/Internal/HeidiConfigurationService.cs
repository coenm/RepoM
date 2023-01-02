namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;
using Microsoft.Extensions.Logging;
using RepoM.Plugin.Heidi.Internal.Config;

internal sealed class HeidiConfigurationService : IDisposable
{
    const string PATH = "C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable\\";
    const string FILENAME = "portable_settings.txt";

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly HeidiPortableConfigReader _reader;
    private IFileSystemWatcher? _fileWatcher;
    private IDisposable? _eventSubscription;
    private Dictionary<string, List<RepomHeidiConfig>> _repositoryHeidiConfigs = new();

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
        if (_fileSystem.File.Exists(Path.Combine(PATH, FILENAME)))
        {
            _fileWatcher = _fileSystem.FileSystemWatcher.New(PATH, FILENAME);
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileWatcher.EnableRaisingEvents = true;
            _eventSubscription = Observable
                .FromEventPattern<FileSystemEventArgs>(_fileWatcher, nameof(_fileWatcher.Changed))
                .ObserveOn(Scheduler.Default)
                .Select(e => e.EventArgs)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(OnNext);

            Task.Run(() => OnNext(new FileSystemEventArgs(WatcherChangeTypes.Changed, PATH, FILENAME)));
        }
        else
        {
            _logger.LogWarning("Heidi module not enabled because file not found.");
        }

        return Task.CompletedTask;
    }

    private async void OnNext(FileSystemEventArgs e)
    {
        try
        {
            _logger.LogDebug("File changed '{name}' '{type}' '{fullPath}'", e.Name, e.ChangeType, e.FullPath);

            Dictionary<string, RepomHeidiConfig> config = await _reader.ReadConfigsAsync(e.FullPath);
            
            var newResult = new Dictionary<string, List<RepomHeidiConfig>>();

            foreach (KeyValuePair<string, RepomHeidiConfig> item in config)
            {
                foreach (var r in item.Value.Repositories)
                {
                    newResult.TryAdd(r, new List<RepomHeidiConfig>());
                    newResult[r].Add(item.Value);
                }
            }

            _repositoryHeidiConfigs = newResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not process Heidi configuration {message}", exception.Message);
        }
    }

    public IEnumerable<RepomHeidiConfig> GetByRepository(IRepository repository)
    {
        Remote? origin = repository.Remotes.FirstOrDefault(x => "Origin".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));

        if (origin == null)
        {
            return Array.Empty<RepomHeidiConfig>();
        }

        if (_repositoryHeidiConfigs.TryGetValue(origin.Name, out List<RepomHeidiConfig>? configs))
        {
            return configs.AsReadOnly();
        }

        return Array.Empty<RepomHeidiConfig>();
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
        _fileWatcher?.Dispose();
    }
}