namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Action to excute a command (related the the repository)
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionCommandV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "command@1";

    /// <summary>
    /// The command to execute.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Command { get; set; }

    /// <summary>
    /// Arguments for the command.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Arguments { get; set; }
}