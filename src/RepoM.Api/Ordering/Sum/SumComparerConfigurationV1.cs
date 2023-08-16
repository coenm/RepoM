namespace RepoM.Api.Ordering.Sum;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Compares two repositories by the sum of the results of the comparers.
/// </summary>
public sealed class SumComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "sum-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// A list of comparers. The sum of the results of the comparers will be used as final result.
    /// </summary>
    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}