namespace RepoM.Api.Ordering.Az;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class AlphabetComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public string? Property { get; set; }

    public int Weight { get; set; }
}