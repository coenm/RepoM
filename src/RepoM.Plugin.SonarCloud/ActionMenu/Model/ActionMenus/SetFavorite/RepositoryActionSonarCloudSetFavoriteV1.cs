namespace RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

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
    public Text Name { get; set; } = null!;

    /// <summary>
    /// The SonarCloud project key.
    /// </summary>
    [Required]
    [Text]
    public Text Project { get; set; } = null!;

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = true;

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Project}";
    }
}