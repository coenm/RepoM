namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

public class LabelScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public int Weight { get; set; }

    public string? Label { get; set; }
}