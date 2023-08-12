namespace RepoM.Plugin.Heidi.ActionProvider;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to list heidi databases and show action menus for them.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionHeidiDatabasesV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "heidi-databases@1";

    /// <summary>
    /// Repository key.
    /// If not provided, the repository `Remote.Origin.Name` is used as selector.
    /// </summary>
    [PropertyType(typeof(string))]
    public string? Key { get; set; }

    /// <summary>
    /// The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Executable { get; set; }
}