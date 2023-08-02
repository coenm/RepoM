namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionForEachV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "foreach@1";

    /// <summary>
    /// List of actions for the enumeration.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public List<RepositoryAction> Actions { get; set; } = new List<RepositoryAction>();

    /// <summary>
    /// The name of the variable to access to current enumeration of the <see cref="Enumerable"/> items.
    /// </summary>
    // [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Variable { get; set; }

    /// <summary>
    /// The list of items to enumerate on. TODO.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Enumerable { get; set; }

    /// <summary>
    /// Contition to skip the current enumeration.
    /// </summary>
    [EvaluatedProperty]
    // [Required]
    [PropertyType(typeof(string))]
    public string? Skip { get; set; }
}