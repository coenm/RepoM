namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionExecutableV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "executable@1";

    /// <summary>
    /// 
    /// </summary>
    // [EvaluatedProperty] //TODO
    [Required]
    [PropertyType(typeof(string))]
    public List<string> Executables { get; set; } = new List<string>();

    /// <summary>
    /// Arguments for the executable.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Arguments { get; set; }
}