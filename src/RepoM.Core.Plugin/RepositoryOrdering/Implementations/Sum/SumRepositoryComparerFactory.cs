namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;

using System.Collections.Generic;

public class SumRepositoryComparerFactory : IRepositoryComparerFactory<SumComparerConfigurationV1>
{
    public IRepositoryComparer Create(SumComparerConfigurationV1 configuration)
    {
        // configuration.ScoreProvider
        return new SumCompositionComparer(new List<IRepositoryComparer>());
    }
}