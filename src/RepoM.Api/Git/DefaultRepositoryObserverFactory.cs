namespace RepoM.Api.Git;

using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystem _fileSystem;

    public DefaultRepositoryObserverFactory(ILoggerFactory loggerFactory, IFileSystem fileSystem)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public IRepositoryObserver Create()
    {
        ILogger<DefaultRepositoryObserver> logger = _loggerFactory.CreateLogger<DefaultRepositoryObserver>();
        return new DefaultRepositoryObserver(logger, _fileSystem);
    }
}