namespace RepoM.Api.Ordering.IsPinned;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Repository scorer based on the pinned state of a repository.
/// </summary>
public sealed class IsPinnedScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public const string TYPE_VALUE = "is-pinned-scorer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// The weight of this scorer. The higher the weight, the higher the impact.
    /// </summary>
    public int Weight { get; set; }
}