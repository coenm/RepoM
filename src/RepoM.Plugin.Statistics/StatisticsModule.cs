namespace RepoM.Plugin.Statistics;

using System;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Statistics.Interface;

[UsedImplicitly]
internal class StatisticsModule : IModule
{
    private readonly IStatisticsService _service;
    private readonly IStatisticsConfiguration _configuration;
    private readonly IClock _clock;
    private readonly IAppDataPathProvider _pathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private string _basePath = string.Empty;
    private IDisposable? _disposable;
    private readonly JsonSerializerSettings _settings;

    public StatisticsModule(
        IStatisticsService service,
        IStatisticsConfiguration configuration,
        IClock clock,
        IAppDataPathProvider pathProvider,
        IFileSystem fileSystem,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            };
    }

    public async Task StartAsync()
    {
        _basePath = _fileSystem.Path.Combine(_pathProvider.AppDataPath, "Module", "Statistics");
        
        _disposable = WriteEventsToFile();

        await ProcessEventsFromFile().ConfigureAwait(false);

        _ = Task.Run(RemoveOldFilesAsync);
    }

    public Task StopAsync()
    {
        _disposable?.Dispose();
        return Task.CompletedTask;
    }

    private async Task RemoveOldFilesAsync()
    {
        if (!_fileSystem.Directory.Exists(_basePath))
        {
            return;
        }

        IOrderedEnumerable<string> orderedEnumerable = _fileSystem.Directory.GetFiles(_basePath, "statistics.v1.*.json").OrderBy(f => f);

        DateTime threshold = _clock.Now.AddDays(-1 * _configuration.RetentionDays);

        foreach (var file in orderedEnumerable)
        {
            IEvent[] events = Array.Empty<IEvent>();

            try
            {
                var json = await _fileSystem.File.ReadAllTextAsync(file, CancellationToken.None).ConfigureAwait(false);
                events = JsonConvert.DeserializeObject<IEvent[]>(json, _settings) ?? Array.Empty<IEvent>();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not read or deserialize data from '{Filename}'. {Message}", file, e.Message);
            }

            if (Array.TrueForAll(events, item => item.Timestamp <= threshold))
            {
                try
                {
                    _logger.LogDebug("Remove old Statistics file '{Filename}'", file);
                    _fileSystem.File.Delete(file);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not delete '{Filename}'. {Message}", file, e.Message);
                }
            }
        }
    }

    private async Task ProcessEventsFromFile()
    {
        if (!_fileSystem.Directory.Exists(_basePath))
        {
            return;
        }

        IOrderedEnumerable<string> orderedEnumerable = _fileSystem.Directory.GetFiles(_basePath, "statistics.v1.*.json").OrderBy(f => f);

        foreach (var file in orderedEnumerable)
        {
            IEvent[] list = Array.Empty<IEvent>();

            try
            {
                var json = await _fileSystem.File.ReadAllTextAsync(file, CancellationToken.None).ConfigureAwait(false);
                list = JsonConvert.DeserializeObject<IEvent[]>(json, _settings) ?? Array.Empty<IEvent>();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not read or deserialize data from '{Filename}'. {Message}", file, e.Message);
            }

            foreach (IEvent item in list)
            {
                _service.Apply(item);
            }
        }
    }

    private IDisposable WriteEventsToFile()
    {
        TimeSpan buffer = _configuration.PersistenceBuffer;
        if (buffer < TimeSpan.FromSeconds(10))
        {
            buffer = TimeSpan.FromSeconds(10);
        }

        return _service
               .Events
               .ObserveOn(Scheduler.Default)
               .Buffer(buffer)
               .Subscribe(data =>
                   {
                       IEvent[] events = [.. data, ];
                       if (events.Length == 0)
                       {
                           return;
                       }
                    
                       var json = JsonConvert.SerializeObject(events, _settings);
                       var filename = _fileSystem.Path.Combine(_basePath, $"statistics.v1.{_clock.Now:yyyy-MM-dd HH.mm.ss}.json");

                       if (!_fileSystem.Directory.Exists(_basePath))
                       {
                           try
                           {
                               _logger.LogDebug("Try Create directory '{BasePath}'.", _basePath);
                               _fileSystem.Directory.CreateDirectory(_basePath);
                           }
                           catch (Exception e)
                           {
                               _logger.LogError(e, "Could not create directory '{BasePath}'. {Message}", _basePath, e.Message);
                           }
                       }

                       if (!_fileSystem.Directory.Exists(_basePath))
                       {
                           return;
                       }

                       try
                       {
                           _fileSystem.File.WriteAllText(filename, json);
                       }
                       catch (Exception e)
                       {
                           _logger.LogError(e, "Could not write json to '{Filename}'. {Message}", filename, e.Message);
                       }
                   });
    }
}