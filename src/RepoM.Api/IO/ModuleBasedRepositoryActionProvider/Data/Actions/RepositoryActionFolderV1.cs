namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// TODO
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
    [PropertyType(typeof(List<RepositoryAction>))] //todo
    public List<RepositoryAction> Items { get; set; } = new List<RepositoryAction>();


    //  TODo Obsolete?
    /// <summary>
    /// Defere the menu. This speedsup the process but might not work with variables correctly.
    /// </summary>
    [EvaluatedProperty]
    // [Required]
    [PropertyType(typeof(bool))] //todo
    public string? IsDeferred { get; set; }
}