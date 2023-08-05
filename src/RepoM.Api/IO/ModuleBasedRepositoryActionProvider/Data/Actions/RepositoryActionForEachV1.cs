namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Action to create repeated actions based on a variable.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionForEachV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "foreach@1";

    /// <summary>
    /// The list of items to enumerate on.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Enumerable { get; set; }

    /// <summary>
    /// The name of the variable to access to current enumeration of the <see cref="Enumerable"/> items. For each iteration, the variable `{var.name}` has the value of the current iteration.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Variable { get; set; }
    
    /// <summary>
    /// Predicate to skip the current item.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Skip { get; set; }

    /// <summary>
    /// List of repeated actions.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(IEnumerable<RepositoryAction>))]
    public List<RepositoryAction> Actions { get; set; } = new List<RepositoryAction>();
}