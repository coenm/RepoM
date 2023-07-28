namespace RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Configuration to score a single repo
/// </summary>
public interface IRepositoryScorerConfiguration
{
    string Type { get; set; }
}