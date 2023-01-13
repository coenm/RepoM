namespace RepoM.Plugin.Heidi.ActionProvider;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionHeidiDatabasesV1 : RepositoryAction
{
    /// <summary>
    /// Repository key (optional).
    /// If not provided, the Remote.Origin.Name is used as selector.
    /// </summary>
    public string? Key { get; set; }


    /// <summary>
    /// Heidi Sql executable path. (Optional)
    /// </summary>
    public string? Executable { get; set; }
}