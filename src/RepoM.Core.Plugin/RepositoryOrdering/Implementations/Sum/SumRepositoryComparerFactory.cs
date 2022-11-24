namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;

using System;
using System.Linq;

public class SumRepositoryComparerFactory : IRepositoryComparerFactory<SumComparerConfigurationV1>
{
    private readonly IRepositoryComparerFactory _factory;

    public SumRepositoryComparerFactory(IRepositoryComparerFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IRepositoryComparer Create(SumComparerConfigurationV1 configuration)
    {
        return new SumCompositionComparer(configuration.Comparers.Select(c => _factory.Create(c)));
    }
}