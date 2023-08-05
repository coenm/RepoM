namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionFolderV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "folder@1";

    /// <summary>
    /// Menu items.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(List<RepositoryAction>))]
    public List<RepositoryAction> Items { get; set; } = new List<RepositoryAction>();


    // //  TODO Obsolete?
    // /// <summary>
    // /// Defere the menu. This speedsup the process but might not work with variables correctly.
    // /// </summary>
    // [EvaluatedProperty]
    // // [Required]
    // [PropertyType(typeof(bool))] //todo
    public string? IsDeferred { get; set; }
}