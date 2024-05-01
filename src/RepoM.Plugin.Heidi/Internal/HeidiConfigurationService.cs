namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    private readonly IHeidiRepositoryExtractor _repositoryExtractor;
    private readonly IHeidiSettings _settings;
    private IFileSystemWatcher? _fileWatcher;
    private IDisposable? _eventSubscription;
    private Dictionary<string, List<RepositoryHeidiConfiguration>> _repositoryHeidiConfigs = new();
    private ImmutableArray<HeidiSingleDatabaseConfiguration> _rawDatabases;
    private string? _heidiConfigFile;

    public event EventHandler? ConfigurationUpdated;

    public HeidiConfigurationService(
        ILogger logger,
        IFileSystem fileSystem,
        IHeidiPortableConfigReader reader,
        IHeidiRepositoryExtractor repositoryExtractor,
        IHeidiSettings settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _repositoryExtractor = repositoryExtractor ?? throw new ArgumentNullException(nameof(repositoryExtractor));
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

    public ImmutableArray<HeidiSingleDatabaseConfiguration> GetAllDatabases()
    {
        return _rawDatabases;
    }

    public IEnumerable<RepositoryHeidiConfiguration> GetByRepository(IRepository repository)
    {
        Remote? origin = repository.Remotes.Find(x => "Origin".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));

        if (origin == null)
        {
            return Array.Empty<RepositoryHeidiConfiguration>();
        }

        return GetByKey(origin.Name);
    }

    public IEnumerable<RepositoryHeidiConfiguration> GetByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Array.Empty<RepositoryHeidiConfiguration>();
        }

        if (_repositoryHeidiConfigs.TryGetValue(key, out List<RepositoryHeidiConfiguration>? configs))
        {
            return configs.OrderBy(x => x.Order).ToArray();
        }

        return Array.Empty<RepositoryHeidiConfiguration>();
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
            _logger.LogDebug("Filename {Name} {Type} ({FullPath})", e.Name, e.ChangeType, e.FullPath);

            // for now, check exact path and file
            if (!e.FullPath.Equals(_heidiConfigFile, StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogWarning("Filename updated but wasn't configured file, '{Configured}', '{Updated}'", _heidiConfigFile, e.FullPath);
                return;
            }

            List<HeidiSingleDatabaseConfiguration> heidiDatabases = await _reader.ParseAsync(e.FullPath).ConfigureAwait(false);
            var repoHeids = new List<RepoHeidi>();

            foreach (HeidiSingleDatabaseConfiguration c in heidiDatabases)
            {
                if (_repositoryExtractor.TryExtract(c, out RepoHeidi? repoHeidi))
                {
                    repoHeids.Add(repoHeidi.Value);
                }
            }

            var newResult = new Dictionary<string, List<RepositoryHeidiConfiguration>>();
            foreach (RepoHeidi repository in repoHeids)
            {
                var item = new RepositoryHeidiConfiguration(
                    repository,
                    heidiDatabases.Single(x => x.Key.Equals(repository.HeidiKey)),
                    e.FullPath);
                newResult.TryAdd(repository.Repository, new List<RepositoryHeidiConfiguration>());
                newResult[repository.Repository].Add(item);
            }

            _rawDatabases = [..heidiDatabases];
            _repositoryHeidiConfigs = newResult;
            ConfigurationUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not process Heidi configuration {Message}", exception.Message);
        }
    }
}