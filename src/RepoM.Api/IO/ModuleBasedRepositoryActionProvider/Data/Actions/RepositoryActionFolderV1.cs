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

    // This property has no xml documentation because then it is skipped when generating the documentation.
    // IsDeferred is used to indicate that the items in the folder are genereted lazy. This is used to improve performance.
    // The problem is that variables at this deferred geneartion are not available anymore resulting in unwanted behaviour.
    // At this moment, IsDeferred is not suggested to be used.
    public string? IsDeferred { get; set; }
}