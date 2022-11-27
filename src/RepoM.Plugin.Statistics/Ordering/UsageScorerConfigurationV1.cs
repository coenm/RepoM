namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class UsageScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public UsageScorerConfigurationV1()
    {
    }

    public int Weight { get; set; }
}