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
using RepoM.Plugin.Heidi.Interface;

internal sealed class HeidiConfigurationService : IHeidiConfigurationService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPortableConfigReader _reader;
    private readonly IHeidiSettings _settings;
    private IFileSystemWatcher? _fileWatcher;
    private IDisposable? _eventSubscription;
    private Dictionary<string, List<HeidiConfiguration>> _repositoryHeidiConfigs = new();
    private string? _heidiConfigFile;

    public HeidiConfigurationService(
        ILogger logger,
        IFileSystem fileSystem,
        IHeidiPortableConfigReader reader,
        IHeidiSettings settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public Task InitializeAsync()
    {
        _heidiConfigFile = Path.Combine(_settings.ConfigPath, _settings.ConfigFilename);
        if (_fileSystem.File.Exists(_heidiConfigFile))
        {
            _fileWatcher = _fileSystem.FileSystemWatcher.New(_settings.ConfigPath, _settings.ConfigFilename);
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileWatcher.EnableRaisingEvents = true;
            _eventSubscription = Observable
                .FromEventPattern<FileSystemEventArgs>(_fileWatcher, nameof(_fileWatcher.Changed))
                .ObserveOn(Scheduler.Default)
                .Select(e => e.EventArgs)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(OnFileUpdate);

            Task.Run(() => OnFileUpdate(new FileSystemEventArgs(WatcherChangeTypes.Changed, _settings.ConfigPath, _settings.ConfigFilename)));
        }
        else
        {
            _logger.LogWarning("Heidi module not enabled because file not found.");
        }

        return Task.CompletedTask;
    }

    public IEnumerable<HeidiConfiguration> GetByRepository(IRepository repository)
    {
        Remote? origin = repository.Remotes.FirstOrDefault(x => "Origin".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));

        if (origin == null)
        {
            return Array.Empty<HeidiConfiguration>();
        }

        return GetByKey(origin.Name);
    }

    public IEnumerable<HeidiConfiguration> GetByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Array.Empty<HeidiConfiguration>();
        }

        if (_repositoryHeidiConfigs.TryGetValue(key, out List<HeidiConfiguration>? configs))
        {
            return configs.OrderBy(x => x.Order).ToArray();
        }

        return Array.Empty<HeidiConfiguration>();
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
        _fileWatcher?.Dispose();
    }

    private async void OnFileUpdate(FileSystemEventArgs e)
    {
        try
        {
            _logger.LogDebug("File changed '{name}' '{type}' '{fullPath}'", e.Name, e.ChangeType, e.FullPath);

            // for now, check exact path and file
            if (!e.FullPath.Equals(_heidiConfigFile, StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogWarning("File updated but wasn't configured file, '{configured}', '{updated}'", _heidiConfigFile, e.FullPath);
                return;
            }

            Dictionary<string, RepomHeidiConfig> config = await _reader.ReadConfigsAsync(e.FullPath).ConfigureAwait(false);
            
            var newResult = new Dictionary<string, List<HeidiConfiguration>>();

            foreach (RepomHeidiConfig currentConfig in config.Select(c => c.Value))
            {
                var heidiConfig = new HeidiConfiguration
                    {
                        Name = currentConfig.Name,
                        Description = currentConfig.HeidiKey,
                        Environment = currentConfig.Environment,
                        Order = currentConfig.Order,
                    };

                foreach (var repository in currentConfig.Repositories)
                {
                    newResult.TryAdd(repository, new List<HeidiConfiguration>());
                    newResult[repository].Add(heidiConfig);
                }
            }

            _repositoryHeidiConfigs = newResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not process Heidi configuration {message}", exception.Message);
        }
    }
}