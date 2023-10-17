namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Plugin.AzureDevOps.Internal;

internal class RepositoryActionAzureDevOpsCreatePullRequestV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionAzureDevOpsCreatePullRequestV1>
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly ILogger _logger;

    public RepositoryActionAzureDevOpsCreatePullRequestV1Mapper(IAzureDevOpsPullRequestService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionAzureDevOpsCreatePullRequestV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (repository is not Repository repo)
        {
            // todo
            yield break;
        }

        if (repo.HasLocalChanges)
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


        if (action.AutoComplete == null)
        {
            yield return new UserInterfaceRepositoryAction(name, repository)
                {
                    RepositoryCommand = new DelegateRepositoryCommand((_, _) =>
                        {
                            _service.CreatePullRequestAsync(
                                repository,
                                projectId,
                                action.ReviewerIds,
                                toBranch,
                                pullRequestTitle,
                                draft,
                                includeWorkItems,
                                openInBrowser)
                            .GetAwaiter().GetResult();
                        }),
                    ExecutionCausesSynchronizing = false,
                };
        }
        else
        {
            var deleteSourceBranch = await action.AutoComplete.DeleteSourceBranch.EvaluateAsync(context).ConfigureAwait(false);
            var transitionWorkItems = await action.AutoComplete.TransitionWorkItems.EvaluateAsync(context).ConfigureAwait(false);

            yield return new UserInterfaceRepositoryAction(name, repository)
                {
                    RepositoryCommand = new DelegateRepositoryCommand((_, _) =>
                        {
                            _service.CreatePullRequestWithAutoCompleteAsync(
                                repository,
                                projectId,
                                action.ReviewerIds,
                                toBranch,
                                (int)action.AutoComplete.MergeStrategy,
                                pullRequestTitle,
                                draft,
                                includeWorkItems,
                                openInBrowser,
                                deleteSourceBranch,
                                transitionWorkItems)
                            .GetAwaiter().GetResult();
                        }),
                    ExecutionCausesSynchronizing = false,
                };
        }
    }
}