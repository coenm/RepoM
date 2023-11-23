namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.RepositoryActions;

public interface IRepositoryActionProvider
{
    IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository);
}

public sealed class LoggingRepositoryActionProviderDecorator : IRepositoryActionProvider
{
    private readonly IRepositoryActionProvider _provider;

    public LoggingRepositoryActionProviderDecorator(IRepositoryActionProvider provider, ILogger logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        _ = logger ?? throw new ArgumentNullException(nameof(logger));

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogInformation("IRepositoryActionProvider logging enabled.");
            _provider = new LoggingDecorator(provider, logger);
        }
        else
        {
            logger.LogInformation("IRepositoryActionProvider logging disabled.");
        }
    }

    public IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository)
    {
        return _provider.GetContextMenuActions(repository);
    }
    
    private sealed class LoggingDecorator : IRepositoryActionProvider
    {
        private readonly IRepositoryActionProvider _provider;
        private readonly ILogger _logger;

        public LoggingDecorator(IRepositoryActionProvider provider, ILogger logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository)
        {
            using IDisposable _ = Measure(repository);
            return _provider.GetContextMenuActions(repository).ToArray();
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
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                _logger.LogDebug("GetContextMenuActions {repository} took {time}", _repo.Name, _sw.Elapsed);
            }
        }
    }
}