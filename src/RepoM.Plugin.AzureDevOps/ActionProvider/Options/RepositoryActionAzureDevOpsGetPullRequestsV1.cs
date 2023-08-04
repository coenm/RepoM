namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAzureDevOpsGetPullRequestsV1 : RepositoryActionAzureDevOpsBase
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "azure-devops-get-prs@1";

    /// <summary>
    /// The repository Id.
    /// </summary>
    [EvaluatedProperty]
    // [Required]
    [PropertyType(typeof(string))]
    public string? RepoId { get; set; }

    /// <summary>
    /// When no pull requests are available, this property is used to determine if no or a message item is showed.
    /// </summary>
    [EvaluatedProperty]
    // [Required]
    [PropertyType(typeof(string))]
    public string? ShowWhenEmpty { get; set; }
}