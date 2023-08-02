namespace RepoM.Plugin.Heidi.ActionProvider;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// TODO
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
    /// If not provided, the Remote.Origin.Name is used as selector.
    /// </summary>
    [EvaluatedProperty] // todo ?
    // [Required]
    [PropertyType(typeof(string))]
    public string? Key { get; set; }

    /// <summary>
    /// Heidi Sql executable path. If not provided, the `TODO` is used.
    /// </summary>
    [EvaluatedProperty] // todo check
    // [Required]
    [PropertyType(typeof(string))]
    public string? Executable { get; set; }
}