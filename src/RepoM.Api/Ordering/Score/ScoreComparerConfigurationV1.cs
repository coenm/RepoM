namespace RepoM.Api.Ordering.Score;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Compares two repositories by a repository score. The calculation of the repository score is defined in the score provider.
/// </summary>
public class ScoreComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "score-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// The score provider to calculate a score for a repository.
    /// </summary>
    public IRepositoryScorerConfiguration? ScoreProvider { get; set; }
}