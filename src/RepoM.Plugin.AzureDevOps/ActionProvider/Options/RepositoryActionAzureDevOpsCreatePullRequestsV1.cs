namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using System.Collections.Generic;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action menu item to create a pull request in Azure Devops.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAzureDevOpsCreatePullRequestsV1 : RepositoryActionAzureDevOpsBase
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "azure-devops-create-prs@1";

    /// <summary>
    /// TODO
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public string? PrTitle { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public string ToBranch { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public List<string> ReviewerIds { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public bool DraftPr { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    public bool IncludeWorkItems { get; set; } = true;

    /// <summary>
    /// TODO
    /// </summary>
    public bool OpenInBrowser { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
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