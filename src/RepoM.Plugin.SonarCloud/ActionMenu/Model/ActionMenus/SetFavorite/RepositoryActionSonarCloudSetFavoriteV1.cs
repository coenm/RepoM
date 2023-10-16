namespace RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to mark a repository as favorite within SonarCloud.
/// </summary>
internal sealed class RepositoryActionSonarCloudSetFavoriteV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "sonarcloud-set-favorite@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Name { get; init; } = new RenderString(); // todo nullable?

    /// <summary>
    /// The SonarCloud project key.
    /// </summary>
    [Required]
    [Render]
    public RenderString Project { get; init; } = new RenderString(); // todo nullable?

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Project}";
    }
}