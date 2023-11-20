namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.Git;

public interface IRepositoryTagsFactory
{
    Task<IEnumerable<string>> GetTagsAsync(Repository repository);
}

[UsedImplicitly]
public sealed class LoggingRepositoryTagsFactoryDecorator : IRepositoryTagsFactory
{
    private readonly IRepositoryTagsFactory _provider;

    public LoggingRepositoryTagsFactoryDecorator(IRepositoryTagsFactory provider, ILogger logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        _ = logger ?? throw new ArgumentNullException(nameof(logger));

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogInformation("IRepositoryTagsFactory logging enabled.");
            _provider = new LoggingDecorator(provider, logger);
        }
        else
        {
            logger.LogInformation("IRepositoryTagsFactory logging disabled.");
        }
    }

    public Task<IEnumerable<string>> GetTagsAsync(Repository repository)
    {
        return _provider.GetTagsAsync(repository);
    }
   
    private sealed class LoggingDecorator : IRepositoryTagsFactory
    {
        private readonly IRepositoryTagsFactory _provider;
        private readonly ILogger _logger;

        public LoggingDecorator(IRepositoryTagsFactory provider, ILogger logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<string>> GetTagsAsync(Repository repository)
        {
            using IDisposable _ = Measure(repository);
            return await _provider.GetTagsAsync(repository).ConfigureAwait(false);
        }
        
        private IDisposable Measure(Repository repository)
        {
            return new MeasureLog(_logger, repository);
        }

        private sealed class MeasureLog : IDisposable
        {
            private readonly Stopwatch _sw;
            private readonly ILogger _logger;
            private readonly Repository _repo;

            public MeasureLog(ILogger logger, Repository repo)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _repo = repo ?? throw new ArgumentNullException(nameof(repo));
                _logger.LogDebug("GetTags {repository} Start", _repo.Name);
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                _logger.LogDebug("GetTags {repository} took {time}", _repo.Name, _sw.Elapsed);
            }
        }
    }
}