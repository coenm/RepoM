namespace RepoM.Api.Ordering.Composition;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class CompositionComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public const string TYPE_VALUE = "composition@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}