namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Ignore;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
/// To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionIgnoreV1 : IMenuAction, IContext, IName
{
    public const string TYPE_VALUE = "ignore-repository@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render("Ignore")]
    public RenderString Name { get; init; } = new ScribanRenderString(); // todo nullable?

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public Context? Context { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}