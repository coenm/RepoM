namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Fetch;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionGitFetchV1 : IMenuAction, IOptionalName
{
    public const string TYPE_VALUE = "git-fetch@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render("Fetch")]
    public RenderString Name { get; init; } = new ScribanRenderString();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}