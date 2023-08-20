namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action menu item to create a pull request in Azure Devops.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAzureDevOpsCreatePullRequestsV2 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "azure-devops-create-prs@2";

    /// <summary>
    /// The azure devops project id. If none given, the default project id, set in the plugin configuration, will be used. At least one is required.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? ProjectId { get; set; }

    /// <summary>
    /// Pull Request title. When not provided, the title will be defined based on the branch name.
    /// Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Title { get; set; }

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Required]
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? ToBranch { get; set; } = string.Empty;

    /// <summary>
    /// Required reviewers. An id should be a valid Azure DevOps user id (ie. GUID), or email address known in Azure Devops.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public string? Reviewer { get; set; }

    /// <summary>
    /// List of required reviewers. An id should be a valid Azure DevOps user id (ie. GUID), or email address known in Azure Devops.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(List<string>))]
    public List<string> Reviewers { get; set; } = new();

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Required]
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(false)]
    public string? IsDraft { get; set; } = "false";

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages.
    /// </summary>
    [Required]
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public string? IncludeWorkItems { get; set; } = "true";

    /// <summary>
    /// Boolean specifying if the Pull request should be opened in the browser after creation.
    /// </summary>
    [Required]
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(default)]
    public string? OpenInBrowser { get; set; }

    /// <summary>
    /// Auto complete options for the pull request. If not set, autocomplete will be `off`.
    /// </summary>
    [PropertyType(typeof(RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV2))]
    public RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV2? AutoComplete { get; set; }
}

/// <summary>
/// Auto complete options.
/// </summary>
public class RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV2
{
    /// <summary>
    /// The merge strategy. Possible values are `NoFastForward`, `Squash` and `Rebase`, and `RebaseMerge`.
    /// </summary>
    [PropertyType(typeof(RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy))]
    [PropertyDefaultTypedValueAttribute<RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy>(RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy.NoFastForward)]
    public RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy MergeStrategy { get; set; } = RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy.NoFastForward;

    /// <summary>
    /// Boolean specifying if the source branche should be deleted afer completion.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public string? DeleteSourceBranch { get; set; } = "true";

    /// <summary>
    /// Boolean specifying if related workitems should be transitioned to the next state.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public string? TransitionWorkItems { get; set; } = "true";
}