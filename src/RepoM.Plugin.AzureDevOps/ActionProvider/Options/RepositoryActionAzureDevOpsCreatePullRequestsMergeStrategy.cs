namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

/// <summary>
/// Merge strategies Azure Devops supports.
/// </summary>
public enum RepositoryActionAzureDevOpsCreatePullRequestsMergeStrategy
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