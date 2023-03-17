namespace RepoM.Plugin.Statistics.Ordering;

using System;
using RepoM.Core.Plugin.RepositoryOrdering;

public sealed class LastOpenedComparerFactory : IRepositoryComparerFactory<LastOpenedConfigurationV1>
{
    private readonly IStatisticsService _service;

    public LastOpenedComparerFactory(IStatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public IRepositoryComparer Create(LastOpenedConfigurationV1 configuration)
    {
        return new LastOpenedComparer(_service, configuration.Weight);
    }
}