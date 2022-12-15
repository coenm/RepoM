namespace RepoM.Api.Ordering.Composition;

using System;
using System.Linq;
using RepoM.Core.Plugin.RepositoryOrdering;

public class CompositionRepositoryComparerFactory : IRepositoryComparerFactory<CompositionComparerConfigurationV1>
{
    private readonly IRepositoryComparerFactory _factory;

    public CompositionRepositoryComparerFactory(IRepositoryComparerFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IRepositoryComparer Create(CompositionComparerConfigurationV1 configuration)
    {
        return new CompositionComparer(configuration.Comparers.Select(c => _factory.Create(c)));
    }
}