namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public sealed class LastOpenedConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "last-opened-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public int Weight { get; set; }
}