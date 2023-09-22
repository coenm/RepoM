namespace RepoM.Api.Git;

using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

public class DefaultRepositoryDetectorFactory : IRepositoryDetectorFactory
{
    private readonly IRepositoryReader _repositoryReader;
    private readonly ILoggerFactory _factory;
    private readonly IFileSystem _fileSystem;

    public DefaultRepositoryDetectorFactory(IRepositoryReader repositoryReader, IFileSystem fileSystem, ILoggerFactory factory)
    {
        _repositoryReader = repositoryReader;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public IRepositoryDetector Create()
    {
        ILogger<DefaultRepositoryDetector> logger = _factory.CreateLogger<DefaultRepositoryDetector>();
        return new DefaultRepositoryDetector(_repositoryReader, _fileSystem, logger);
    }
}