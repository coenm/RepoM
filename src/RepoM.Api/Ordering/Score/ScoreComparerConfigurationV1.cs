namespace RepoM.Api.Ordering.Score;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ScoreComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "score-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public IRepositoryScorerConfiguration? ScoreProvider { get; set; }
}