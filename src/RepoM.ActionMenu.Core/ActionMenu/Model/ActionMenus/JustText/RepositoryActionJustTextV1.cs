namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.JustText;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Textual action to display some text in the action menu.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionJustTextV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "just-text@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <summary>
    /// Show the menu as enabled (clickable) or disabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Enabled { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}