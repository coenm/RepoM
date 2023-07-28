namespace RepoM.Api.Ordering.Sum;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class SumComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "sum-comparer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}