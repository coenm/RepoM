namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using System;

/// <summary>
/// This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
/// </summary>
[Obsolete("Old action menu")]
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAzureDevOpsGetPullRequestsV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "azure-devops-get-prs@1";

    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? ProjectId { get; set; }

    /// <summary>
    /// The repository Id. If not provided, the repository id is located using the remote url.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? RepositoryId { get; set; }

    /// <summary>
    /// When no pull requests are available, this property is used to determine if no or a message item is showed.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    [PropertyDefaultBoolValue(true)]
    public string? ShowWhenEmpty { get; set; }
}