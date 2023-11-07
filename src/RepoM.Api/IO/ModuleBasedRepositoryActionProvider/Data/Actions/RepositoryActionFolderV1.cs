namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

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

    /// <summary>
    /// Menu is deferred. This will speed up visualisation.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(false)]
    public string? IsDeferred { get; set; }
}