namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionForEachV1 : IMenuAction
{
    public const string TYPE_VALUE = "foreach@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public EvaluateBoolean Active { get; init; } = new ScribanEvaluateBoolean(); // todo nullable?
    
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
    [EvaluateToBoolean(false)]
    public EvaluateBoolean Skip { get; init; } = new ScribanEvaluateBoolean();

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