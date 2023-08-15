namespace RepoM.Api.Ordering.Label;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Repository scorer based on the tags of a repository.
/// </summary>
public sealed class TagScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public const string TYPE_VALUE = "tag-scorer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// The weight of this scorer. The higher the weight, the higher the impact.
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// The tag to match on. If the repository has this tag, the score will return the weight, otherwise, `0`.
    /// </summary>
    public string? Tag { get; set; }
}