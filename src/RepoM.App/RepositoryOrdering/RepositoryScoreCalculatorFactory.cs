namespace RepoM.App.RepositoryOrdering;

using System;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;

internal class RepositoryScoreCalculatorFactory : IRepositoryScoreCalculatorFactory
{
    private readonly Container _container;
    private readonly ILogger<RepositoryScoreCalculatorFactory> _logger;

    public RepositoryScoreCalculatorFactory(Container container, ILogger<RepositoryScoreCalculatorFactory> logger)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IRepositoryScoreCalculator Create(IRepositoryScorerConfiguration configuration)
    {
        try
        {
            return CreateInner(configuration);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Could not create a IRepositoryComparer for configuration type '{Configuration}'", configuration);
            throw;
        }
    }

    private IRepositoryScoreCalculator CreateInner(IRepositoryScorerConfiguration configuration)
    {
        Type type = typeof(IRepositoryScoreCalculatorFactory<>).MakeGenericType(configuration.GetType());
        dynamic factory = _container.GetInstance(type);
        return factory.Create((dynamic)configuration);
    }
}