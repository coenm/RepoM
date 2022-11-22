namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Composition;

using System.Collections.Generic;

public class CompositionRepositoryComparerFactory : IRepositoryComparerFactory<CompositionComparerConfigurationV1>
{
    public IRepositoryComparer Create(CompositionComparerConfigurationV1 configuration)
    {
        // configuration.ScoreProvider
        return new CompositionComparer(new List<IRepositoryComparer>());
    }
}