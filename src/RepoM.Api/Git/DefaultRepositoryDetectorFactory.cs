namespace RepoM.Api.Git;

using System;
using Microsoft.Extensions.Logging;

public class DefaultRepositoryDetectorFactory : IRepositoryDetectorFactory
{
    private readonly IRepositoryReader _repositoryReader;
    private readonly ILoggerFactory _factory;

    public DefaultRepositoryDetectorFactory(IRepositoryReader repositoryReader, ILoggerFactory factory)
    {
        _repositoryReader = repositoryReader;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IRepositoryDetector Create()
    {
        return new DefaultRepositoryDetector(_repositoryReader, _factory.CreateLogger<DefaultRepositoryDetector>());
    }
}