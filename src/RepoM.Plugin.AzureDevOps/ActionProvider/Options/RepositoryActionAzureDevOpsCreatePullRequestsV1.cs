namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using System.Collections.Generic;

public class RepositoryActionAzureDevOpsCreatePullRequestsV1 : RepositoryActionAzureDevOpsBase
{
    public string? Title { get; set; }
    public string? PrTitle { get; set; }
    public string ToBranch { get; set; }
    public List<string> ReviewerIds { get; set; }
    public bool DraftPr { get; set; }
    public bool IncludeWorkItems { get; set; } = true;
    public bool OpenInBrowser { get; set; }
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
    public bool Enabled { get; set; }
    public RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategyV1 MergeStrategy { get; set; } = RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategyV1.NoFastForward;
    public bool DeleteSourceBranch { get; set; } = true;
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