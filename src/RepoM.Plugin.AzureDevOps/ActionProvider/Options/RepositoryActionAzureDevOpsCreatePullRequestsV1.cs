namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action menu item to create a pull request in Azure Devops.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAzureDevOpsCreatePullRequestsV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "azure-devops-create-prs@1";

    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? ProjectId { get; set; }

    // todo in v2, should be name (as others are)
    /// <summary>
    /// Menu item title. When not provided, a title will be generated.
    /// This property will be used instead of the Name property.
    /// </summary>
    // [EvaluatedProperty] // todo
    [PropertyType(typeof(string))]
    public string? Title { get; set; }

    /// <summary>
    /// Pull Request title. When not provided, the title will be defined based on the branch name.
    /// Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
    /// </summary>
    [PropertyType(typeof(string))]
    public string? PrTitle { get; set; }

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Required]
    [PropertyType(typeof(string))]
    public string ToBranch { get; set; }

    /// <summary>
    /// List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
    /// </summary>
    [PropertyType(typeof(List<string>))]
    // [PropertyDefaultBoolValue(default)] //todo
    public List<string> ReviewerIds { get; set; }

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Required]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(default)]
    public bool DraftPr { get; set; }

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages.
    /// </summary>
    [Required]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public bool IncludeWorkItems { get; set; } = true;

    /// <summary>
    /// Boolean specifying if the Pull request should be opened in the browser after creation.
    /// </summary>
    [Required]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(default)]
    public bool OpenInBrowser { get; set; }

    /// <summary>
    /// Auto complete options. Please take a look at the same for more information
    /// </summary>
    [Required]
    [PropertyType(typeof(RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV1))]
    public RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV1 AutoComplete { get; set; }

    public RepositoryActionAzureDevOpsCreatePullRequestsV1()
    {
        ToBranch = string.Empty;
        ReviewerIds = new();
        AutoComplete = new();
    }
}

public class RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV1
{
    /// <summary>
    /// TODO
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategyV1 MergeStrategy { get; set; } = RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategyV1.NoFastForward;

    /// <summary>
    /// TODO
    /// </summary>
    public bool DeleteSourceBranch { get; set; } = true;

    /// <summary>
    /// TODO
    /// </summary>
    public bool TransitionWorkItems { get; set; } = true;
}

public enum RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategyV1
{
    /// <summary>
    /// A two-parent, no-fast-forward merge. The source branch is unchanged. This is the default behavior.
    /// </summary>
    NoFastForward = 1,

    /// <summary>
    /// Put all changes from the pull request into a single-parent commit.
    /// </summary>
    Squash,

    /// <summary>
    /// Rebase the source branch on top of the target branch HEAD commit, and fast-forward the target branch.
    /// The source branch is updated during the rebase operation.
    /// </summary>
    Rebase,

    /// <summary>
    /// Rebase the source branch on top of the target branch HEAD commit, and create a two-parent,
    /// no-fast-forward merge. The source branch is updated during the rebase operation.
    /// </summary>
    RebaseMerge,
}