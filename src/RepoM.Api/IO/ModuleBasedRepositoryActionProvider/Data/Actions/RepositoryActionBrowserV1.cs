namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Action opening a webbrowser with the provided url.
/// </summary>
[RepositoryAction(TYPE)]

public sealed class RepositoryActionBrowserV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "browser@1";
    
    /// <summary>
    /// The url to browse to.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Url { get; set; }
}