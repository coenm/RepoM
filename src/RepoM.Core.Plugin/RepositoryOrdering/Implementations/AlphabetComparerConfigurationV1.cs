namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class AlphabetComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public string? Property { get; set; }

    public int Weight { get; set; }
}