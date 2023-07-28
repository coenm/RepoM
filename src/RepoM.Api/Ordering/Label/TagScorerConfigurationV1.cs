namespace RepoM.Api.Ordering.Label;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class TagScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public const string TYPE_VALUE = "tag-scorer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public int Weight { get; set; }

    public string? Tag { get; set; }
}