namespace RepoM.Plugin.AzureDevOps.RepositoryCommands;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryActions;

public sealed class CreatePullRequestRepositoryCommand : IRepositoryCommand
{
    public CreatePullRequestRepositoryCommand(
        string projectId,
        List<string> reviewerIds,
        string toBranch,
        string pullRequestTitle,
        bool draft,
        bool includeWorkItems,
        bool openInBrowser)
    {
        ProjectId = projectId;
        ReviewerIds = reviewerIds;
        ToBranch = toBranch;
        PullRequestTitle = pullRequestTitle;
        Draft = draft;
        IncludeWorkItems = includeWorkItems;
        OpenInBrowser = openInBrowser;
        AutoComplete = null;
    }

    public CreatePullRequestRepositoryCommand(
        string projectId,
        List<string> reviewerIds,
        string toBranch,
        string pullRequestTitle,
        bool draft,
        bool includeWorkItems,
        bool openInBrowser,
        MergeStrategy autoCompleteMergeStrategy,
        bool deleteSourceBranch,
        bool transitionWorkItems) : this(projectId, reviewerIds, toBranch, pullRequestTitle, draft, includeWorkItems, openInBrowser)
    {
        AutoComplete = new AutoComplete(autoCompleteMergeStrategy, deleteSourceBranch, transitionWorkItems);
    }

    public string ProjectId { get; }

    public List<string> ReviewerIds { get; }

    public string ToBranch { get; }

    public string PullRequestTitle { get; }

    public bool Draft { get; }

    public bool IncludeWorkItems { get; }

    public bool OpenInBrowser { get; }

    public AutoComplete? AutoComplete { get; }


    public enum MergeStrategy
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
}

public class AutoComplete
{
    public AutoComplete(CreatePullRequestRepositoryCommand.MergeStrategy mergeStrategy, bool deleteSourceBranch, bool transitionWorkItems)
    {
        MergeStrategy = mergeStrategy;
        DeleteSourceBranch = deleteSourceBranch;
        TransitionWorkItems = transitionWorkItems;
    }

    public CreatePullRequestRepositoryCommand.MergeStrategy MergeStrategy { get; }

    public bool DeleteSourceBranch { get; }

    public bool TransitionWorkItems { get; }
}