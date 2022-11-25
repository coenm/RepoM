namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class TagScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public int Weight { get; set; }

    public string? Tag { get; set; }
}