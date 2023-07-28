namespace RepoM.Api.Ordering.IsPinned;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class IsPinnedScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public const string TYPE_VALUE = "is-pinned-scorer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public int Weight { get; set; }
}