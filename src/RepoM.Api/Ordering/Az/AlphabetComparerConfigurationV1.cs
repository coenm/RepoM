namespace RepoM.Api.Ordering.Az;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Compares two repositories by a given property.
/// </summary>
public class AlphabetComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "az-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Repository property. Currently, only `Name`, and `Location` are supported.
    /// </summary>
    public string? Property { get; set; }
    
    /// <summary>
    /// The weight of this comparer. The higher the weight, the higher the priority.
    /// </summary>
    public int Weight { get; set; }
}