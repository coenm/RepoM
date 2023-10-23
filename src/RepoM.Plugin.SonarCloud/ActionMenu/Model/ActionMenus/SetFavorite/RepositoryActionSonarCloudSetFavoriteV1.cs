namespace RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to mark a repository as favorite within SonarCloud.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionSonarCloudSetFavoriteV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "sonarcloud-set-favorite@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = new Text(); // todo nullable?

    /// <summary>
    /// The SonarCloud project key.
    /// </summary>
    [Required]
    [Text]
    public Text Project { get; init; } = new Text(); // todo nullable?

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Project}";
    }
}