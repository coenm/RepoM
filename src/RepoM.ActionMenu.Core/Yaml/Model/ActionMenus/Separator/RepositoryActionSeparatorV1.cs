namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Separator;

using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Creates a visual separator in the action menu.
/// </summary>
internal sealed class RepositoryActionSeparatorV1 : IMenuAction, IContext
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE_VALUE = "separator@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public Context? Context { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}