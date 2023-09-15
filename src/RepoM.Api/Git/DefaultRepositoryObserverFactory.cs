namespace RepoM.Api.Git;

using System;
using Microsoft.Extensions.Logging;

public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public DefaultRepositoryObserverFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public IRepositoryObserver Create()
    {
        ILogger<DefaultRepositoryObserver> logger = _loggerFactory.CreateLogger<DefaultRepositoryObserver>();
        return new DefaultRepositoryObserver(logger);
    }
}