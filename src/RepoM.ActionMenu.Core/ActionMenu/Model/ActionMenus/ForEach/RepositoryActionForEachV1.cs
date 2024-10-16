namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.ForEach;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Variable = Interface.YamlModel.Templating.Variable;

/// <summary>
/// Action to create repeated actions based on a variable.
/// </summary>
/// <example>
/// <snippet name='foreach@1-scenario01' mode='snippet' />
/// <snippet name='foreach@1-scenario02' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionForEachV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "foreach@1";
    internal const string EXAMPLE_1 = TYPE_VALUE + "-scenario01";
    internal const string EXAMPLE_2 = TYPE_VALUE + "-scenario02";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; init; }

    /// <summary>
    /// Additional context added for each iteration.
    /// </summary>
    public Context? IterationContext { get; init; }

    /// <summary>
    /// The list of items to enumerate on.
    /// </summary>
    [Required]
    [Variable]
    public Variable Enumerable { get; init; } = new ScribanVariable();

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

    // <inheritdoc cref="IMenuActions.Actions"/>
    /// <summary>
    /// List of repeated actions.
    /// </summary>
    [Required]
    public ActionMenu Actions { get; set; } = [];

    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}