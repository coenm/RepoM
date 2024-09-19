namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Separator;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Creates a visual separator in the action menu.
/// </summary>
/// <example>
/// <snippet name='separator@1-scenario01' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionSeparatorV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "separator@1";
    internal const string EXAMPLE_1 = TYPE_VALUE + "-scenario01";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}