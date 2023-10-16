namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;

using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionFolderV1 : IMenuAction, IName, IMenuActions, IContext, IDeferred
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE_VALUE = "folder@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public ActionMenu? Actions { get; init; }

    [Render]
    public RenderString Name { get; init; } = new();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public Context? Context { get; init; }

    [EvaluateToBoolean(false)]
    public Predicate IsDeferred { get; init; } = new ScribanPredicate();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : #actions: {Actions?.Count ?? 0}";
    }
}