namespace RepoM.Plugin.SonarCloud;

using System.ComponentModel.DataAnnotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionSonarCloudSetFavoriteV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "sonarcloud-set-favorite@1";

    /// <summary>
    /// The SonarCloud Project id.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Project { get; set; }
}