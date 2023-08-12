namespace RepoM.Api.Ordering.Az;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class AlphabetComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "az-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string? Property { get; set; }

    public int Weight { get; set; }
}