namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.GetPullRequests;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Plugin.AzureDevOps.Internal;

internal class RepositoryActionAzureDevOpsGetPullRequestsV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionAzureDevOpsGetPullRequestsV1>
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly ILogger _logger;

    public RepositoryActionAzureDevOpsGetPullRequestsV1Mapper(IAzureDevOpsPullRequestService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionAzureDevOpsGetPullRequestsV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var projectId = await action.ProjectId.RenderAsync(context).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(projectId))
        {
            yield break;
        }

        var repoId = await action.RepositoryId.RenderAsync(context).ConfigureAwait(false);


        List<PullRequest>? pullRequests = null;
        UserInterfaceRepositoryAction? errorAction = null;
        try
        {
            pullRequests = _service.GetPullRequests(repository, projectId, repoId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not grab pull requests.");
            errorAction = new UserInterfaceRepositoryAction($"An error occurred grabbing pull requests. {e.Message}", repository)
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
        }

        if (errorAction != null)
        {
            yield return errorAction;
            yield break;
        }

        if (pullRequests?.Any() ?? false)
        {
            foreach (PullRequest pr in pullRequests)
            {
                yield return new UserInterfaceRepositoryAction(pr.Name, repository)
                    {
                        RepositoryCommand = new BrowseRepositoryCommand(pr.Url),
                    };
            }

            yield break;
        }


        // no pr's found
        // check if user wants a notification
        var showEmpty = await action.ShowWhenEmpty.EvaluateAsync(context).ConfigureAwait(false);
        if (showEmpty)
        {
            yield return new UserInterfaceRepositoryAction("No PRs found.", repository) // todo name
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
        }
    }
}