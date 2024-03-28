namespace RepoM.App.RepositoryOrdering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering;
using System;
using Microsoft.Extensions.Logging;
using SimpleInjector;

internal class RepositoryComparerCompositionFactory : IRepositoryComparerFactory
{
    private readonly Container _container;
    private readonly ILogger<RepositoryComparerCompositionFactory> _logger;

    public RepositoryComparerCompositionFactory(Container container, ILogger<RepositoryComparerCompositionFactory> logger)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IRepositoryComparer Create(IRepositoriesComparerConfiguration configuration)
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

    private IRepositoryComparer CreateInner(IRepositoriesComparerConfiguration configuration)
    {
        Type type = typeof(IRepositoryComparerFactory<>).MakeGenericType(configuration.GetType());
        dynamic factory = _container.GetInstance(type);
        return factory.Create((dynamic)configuration);
    }
}