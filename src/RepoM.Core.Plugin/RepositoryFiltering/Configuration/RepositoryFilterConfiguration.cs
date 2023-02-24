namespace RepoM.Core.Plugin.RepositoryFiltering.Configuration;

using JetBrains.Annotations;

[UsedImplicitly]
public class RepositoryFilterConfiguration
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public QueryConfiguration AlwaysVisible { get; set; } = null!;

    public QueryConfiguration Filter { get; set; } = null!;
}