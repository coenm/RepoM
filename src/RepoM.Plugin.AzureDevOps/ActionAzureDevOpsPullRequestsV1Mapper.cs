namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsPullRequestsV1Mapper : IActionToRepositoryActionMapper
{
    private readonly AzureDevOpsPullRequestService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly ILogger _logger;

    public ActionAzureDevOpsPullRequestsV1Mapper(
        AzureDevOpsPullRequestService service,
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionAzureDevOpsPullRequestsV1;
    }

    public bool CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionAzureDevOpsPullRequestsV1, repository.First());
    }

    private Api.Git.RepositoryAction[] Map(RepositoryActionAzureDevOpsPullRequestsV1? action, Repository repository)
    {
        if (action == null)
        {
            return Array.Empty<Api.Git.RepositoryAction>();
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            return Array.Empty<Api.Git.RepositoryAction>();
        }

        if (string.IsNullOrWhiteSpace(action.ProjectId))
        {
            return Array.Empty<Api.Git.RepositoryAction>();
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);

        string? projectId = null;
        if (action.ProjectId != null)
        {
            projectId = _expressionEvaluator.EvaluateStringExpression(action.ProjectId!, repository);
        }

        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Array.Empty<Api.Git.RepositoryAction>();
        }

        string? repoId = null;
        if (action.RepoId != null)
        {
            repoId = _expressionEvaluator.EvaluateStringExpression(action.RepoId!, repository);
        }

        List<PullRequest> pullRequests;
        try
        {
            pullRequests = _service.GetPullRequests(repository, projectId, repoId);
        }
        catch (Exception e)
        {
            var notificationItem = new Api.Git.RepositoryAction($"An error occurred grabbing pull requests. {e.Message}", repository)
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
            return new[] { notificationItem, };
        }

        if (pullRequests.Any())
        {
            var results = new List<Api.Git.RepositoryAction>(pullRequests.Count);
            results.AddRange(pullRequests.Select(pr => new Api.Git.RepositoryAction(pr.Name, repository)
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
            var notificationItem = new Api.Git.RepositoryAction("No PRs found.", repository)
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
            return new[] { notificationItem, };
        }

        return Array.Empty<Api.Git.RepositoryAction>();
    }
}