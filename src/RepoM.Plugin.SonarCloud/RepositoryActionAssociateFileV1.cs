namespace RepoM.Plugin.SonarCloud;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to mark a repository as favorite within SonarCloud.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionSonarCloudSetFavoriteV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "sonarcloud-set-favorite@1";

    /// <summary>
    /// The SonarCloud project key.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Project { get; set; }
}