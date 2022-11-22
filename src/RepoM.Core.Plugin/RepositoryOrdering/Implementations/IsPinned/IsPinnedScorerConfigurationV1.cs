namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class IsPinnedScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public int Weight { get; set; }
}