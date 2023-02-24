namespace RepoM.Core.Plugin.RepositoryFiltering.Configuration;

using JetBrains.Annotations;

[UsedImplicitly]
public class QueryConfiguration
{
    public string Kind { get; set; } = null!;

    public string Query { get; set; } = null!;
}