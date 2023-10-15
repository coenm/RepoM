namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionForEachV1 : IMenuAction
{
    public const string TYPE_VALUE = "foreach@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string? Active { get; init; }


    /// <summary>
    /// The list of items to enumerate on.
    /// </summary>
    [Required]
    public string? Enumerable { get; set; }

    /// <summary>
    /// The name of the variable to access to current enumeration of the <see cref="Enumerable"/> items. For each iteration, the variable `{var.name}` has the value of the current iteration.
    /// </summary>
    [Required]
    public string? Variable { get; set; }

    /// <summary>
    /// Predicate to skip the current item.
    /// </summary>
    public string? Skip { get; set; }

    /// <summary>
    /// List of repeated actions.
    /// </summary>
    [Required]
    public List<IMenuAction> Actions { get; set; } = new List<IMenuAction>();
    
    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}