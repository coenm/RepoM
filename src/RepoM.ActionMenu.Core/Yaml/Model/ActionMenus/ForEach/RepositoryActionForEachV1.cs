namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using Variable = RepoM.ActionMenu.Interface.YamlModel.Templating.Variable;

[RepositoryAction(TYPE_VALUE)]
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
    [Predicate(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    /// <summary>
    /// The list of items to enumerate on.
    /// </summary>
    [Required]
    [Variable]
    public Variable Enumerable { get; init; } = new ScribanVariable(); // todo enumerable?

    /// <summary>
    /// The name of the variable to access to current enumeration of the <see cref="Enumerable"/> items. For each iteration, the variable `{var.name}` has the value of the current iteration.
    /// </summary>
    [Required]
    public string? Variable { get; init; }

    /// <summary>
    /// Predicate to skip the current item.
    /// </summary>
    [Predicate(false)]
    public Predicate Skip { get; init; } = new ScribanPredicate();

    /// <summary>
    /// List of repeated actions.
    /// </summary>
    [Required]
    public List<IMenuAction> Actions { get; init; } = new List<IMenuAction>();
    
    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}