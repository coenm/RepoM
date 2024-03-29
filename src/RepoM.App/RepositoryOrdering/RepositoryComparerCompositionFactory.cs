namespace RepoM.App.RepositoryOrdering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering;
using System;
using Microsoft.Extensions.Logging;
using SimpleInjector;

internal class RepositoryComparerCompositionFactory : IRepositoryComparerFactory
{
    private readonly Container _container;
    private readonly ILogger _logger;

    public RepositoryComparerCompositionFactory(Container container, ILogger logger)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <exception cref="InvalidOperationException">Thrown when repository comparer cannot be created.</exception>
    public IRepositoryComparer Create(IRepositoriesComparerConfiguration configuration)
    {
        try
        {
            return CreateInner(configuration);
        }
        catch (ActivationException e)
        {
            _logger.LogCritical(e, "Could not create a IRepositoryComparer for configuration type '{Configuration}'", configuration);
            throw new InvalidOperationException($"Could not create a IRepositoryComparer for configuration type '{configuration}'", e);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Factory could not create instance of IRepositoryComparer '{Message}'", e.Message);
            throw new InvalidOperationException($"Factory could not create instance of IRepositoryComparer '{e.Message}'", e);
        }
    }

    private IRepositoryComparer CreateInner(IRepositoriesComparerConfiguration configuration)
    {
        Type type = typeof(IRepositoryComparerFactory<>).MakeGenericType(configuration.GetType());
        dynamic factory = _container.GetInstance(type);
        return factory.Create((dynamic)configuration);
    }
}