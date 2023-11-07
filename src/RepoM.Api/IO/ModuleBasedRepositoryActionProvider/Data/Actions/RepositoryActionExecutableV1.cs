namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionExecutableV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "executable@1";

    /// <summary>
    /// Set of possible executables. The first executable that exists will be used. The paths should absolute.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public List<string> Executables { get; set; } = new List<string>();

    /// <summary>
    /// Arguments for the executable.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    [PropertyDefaultStringValue("")]
    public string? Arguments { get; set; }
}