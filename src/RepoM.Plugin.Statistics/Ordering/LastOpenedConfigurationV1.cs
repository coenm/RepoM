namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Compares two repositories by the timestamp of the last action RepoM performed on the repository.
/// </summary>
public sealed class LastOpenedConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "last-opened-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// The weight of this comparer. The higher the weight, the higher the impact.
    /// </summary>
    public int Weight { get; set; }
}