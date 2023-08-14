namespace RepoM.Api.Ordering.Composition;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Compares two repositories by a composition of comparers.
/// </summary>
public sealed class CompositionComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "composition@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// List of comparers. The first comparer not resulting in `0` will be used as final result.
    /// </summary>
    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}