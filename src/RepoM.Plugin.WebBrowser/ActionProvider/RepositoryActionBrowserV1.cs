namespace RepoM.Plugin.WebBrowser.ActionProvider;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

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
    
    /// <summary>
    /// profile name used to select browser and browser profile
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Profile { get; set; }
}