namespace RepoM.Plugin.AzureDevOps.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using RepoM.Plugin.AzureDevOps.Internal;
using RepositoryAction = Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsGetPullRequestsV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ILogger _logger;

    public ActionAzureDevOpsGetPullRequestsV1Mapper(IAzureDevOpsPullRequestService service, IRepositoryExpressionEvaluator expressionEvaluator, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanMap(RepositoryAction action)
    {
        return action is RepositoryActionAzureDevOpsGetPullRequestsV1;
    }

    public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionAzureDevOpsGetPullRequestsV1, repository);
    }

    private Api.RepositoryActions.RepositoryAction[] Map(RepositoryActionAzureDevOpsGetPullRequestsV1? action, Repository repository)
    {
        if (action == null)
        {
            return Array.Empty<Api.RepositoryActions.RepositoryAction>();
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            return Array.Empty<Api.RepositoryActions.RepositoryAction>();
        }

        if (string.IsNullOrWhiteSpace(action.ProjectId))
        {
            return Array.Empty<Api.RepositoryActions.RepositoryAction>();
        }

        // var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);

        var projectId = _expressionEvaluator.EvaluateStringExpression(action.ProjectId, repository);

        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Array.Empty<Api.RepositoryActions.RepositoryAction>();
        }

        string? repoId = null;
        if (action.RepositoryId != null)
        {
            repoId = _expressionEvaluator.EvaluateStringExpression(action.RepositoryId!, repository);
        }

        List<PullRequest> pullRequests;
        try
        {
            pullRequests = _service.GetPullRequests(repository, projectId, repoId);
        }
        catch (Exception e)
        {
            var notificationItem = new Api.RepositoryActions.RepositoryAction($"An error occurred grabbing pull requests. {e.Message}", repository)
            {
                CanExecute = false,
                ExecutionCausesSynchronizing = false,
            };
            return new[] { notificationItem, };
        }

        if (pullRequests.Any())
        {
            var results = new List<Api.RepositoryActions.RepositoryAction>(pullRequests.Count);
            results.AddRange(pullRequests.Select(pr => new Api.RepositoryActions.RepositoryAction(pr.Name, repository)
            {
                Action = new DelegateAction((_, _) =>
                    {
                        _logger.LogInformation("PullRequest {Url}", pr.Url);
                        ProcessHelper.StartProcess(pr.Url, string.Empty);
                    }),
            }));

            return results.ToArray();
        }

        // no pr's found
        // check if user wants a notification
        if (_expressionEvaluator.EvaluateBooleanExpression(action.ShowWhenEmpty, repository))
        {
            var notificationItem = new Api.RepositoryActions.RepositoryAction("No PRs found.", repository)
            {
                CanExecute = false,
                ExecutionCausesSynchronizing = false,
            };
            return new[] { notificationItem, };
        }

        return Array.Empty<Api.RepositoryActions.RepositoryAction>();
    }
}