namespace RepoM.ActionMenu.Core.ActionMenu.Model.Folder;

using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionFolderV1 : IMenuAction, IName, IMenuActions, IContext, IDeferred
{
    public const string TYPE_VALUE = "folder@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IMenuActions.Actions"/>
    public ActionMenu? Actions { get; set; }

    /// <inheritdoc cref="IName.Name"/>
    [Text]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    [Predicate(false)]
    public Predicate IsDeferred { get; set; } = new ScribanPredicate();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : #actions: {Actions?.Count ?? 0}";
    }
}