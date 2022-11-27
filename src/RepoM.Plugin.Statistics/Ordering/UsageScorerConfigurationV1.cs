namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public sealed class UsageScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public UsageScorerConfigurationV1()
    {
    }

    public int Weight { get; set; }
}