namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.RepositoryCommands;

[UsedImplicitly]
internal class RepositoryActionAzureDevOpsCreatePullRequestV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionAzureDevOpsCreatePullRequestV1>
{
    private readonly ILogger _logger;

    public RepositoryActionAzureDevOpsCreatePullRequestV1Mapper(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionAzureDevOpsCreatePullRequestV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (repository.HasLocalChanges)
        {
            yield break;
        }

        var toBranch = await action.ToBranch.RenderAsync(context).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(toBranch))
        {
            yield break;
        }

        // check if branch exists!
        if (!repository.Branches.Contains(toBranch))
        {
            _logger.LogInformation("Branch {branch} does not exist", toBranch);
            yield break;
        }
        
        if (repository.CurrentBranch.Equals(toBranch, StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var projectId = await action.ProjectId.RenderAsync(context).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(projectId))
        {
            yield break;
        }

        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var pullRequestTitle = await action.PrTitle.RenderAsync(context).ConfigureAwait(false);
        var draft = await action.DraftPr.EvaluateAsync(context).ConfigureAwait(false);
        var includeWorkItems = await action.IncludeWorkItems.EvaluateAsync(context).ConfigureAwait(false);
        var openInBrowser = await action.OpenInBrowser.EvaluateAsync(context).ConfigureAwait(false);

        var reviewers = new List<string>(action.ReviewerIds.Count);
        foreach (Text reviewerId in action.ReviewerIds)
        {
            reviewers.Add(await reviewerId.RenderAsync(context).ConfigureAwait(false));
        }

        if (action.AutoComplete == null)
        {
            yield return new UserInterfaceRepositoryAction(name, repository)
                {
                    RepositoryCommand = new CreatePullRequestRepositoryCommand(
                        projectId,
                        reviewers,
                        toBranch,
                        pullRequestTitle,
                        draft,
                        includeWorkItems,
                        openInBrowser),
                    ExecutionCausesSynchronizing = false,
                };
        }
        else
        {
            var deleteSourceBranch = await action.AutoComplete.DeleteSourceBranch.EvaluateAsync(context).ConfigureAwait(false);
            var transitionWorkItems = await action.AutoComplete.TransitionWorkItems.EvaluateAsync(context).ConfigureAwait(false);

            yield return new UserInterfaceRepositoryAction(name, repository)
                {
                    RepositoryCommand = new CreatePullRequestRepositoryCommand(
                        projectId,
                        reviewers,
                        toBranch,
                        pullRequestTitle,
                        draft,
                        includeWorkItems,
                        openInBrowser,
                        ConvertMergeStrategy(action.AutoComplete.MergeStrategy),
                        deleteSourceBranch,
                        transitionWorkItems),
                    ExecutionCausesSynchronizing = false,
                };
        }
    }

    private static CreatePullRequestRepositoryCommand.MergeStrategy ConvertMergeStrategy(MergeStrategyV1 input)
    {
        return input switch
            {
                MergeStrategyV1.NoFastForward => CreatePullRequestRepositoryCommand.MergeStrategy.NoFastForward,
                MergeStrategyV1.Squash => CreatePullRequestRepositoryCommand.MergeStrategy.Squash,
                MergeStrategyV1.Rebase => CreatePullRequestRepositoryCommand.MergeStrategy.Rebase,
                MergeStrategyV1.RebaseMerge => CreatePullRequestRepositoryCommand.MergeStrategy.RebaseMerge,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
            };
    }
}