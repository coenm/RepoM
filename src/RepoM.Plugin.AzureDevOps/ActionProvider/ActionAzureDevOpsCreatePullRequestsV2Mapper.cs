namespace RepoM.Plugin.AzureDevOps.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using RepoM.Plugin.AzureDevOps.Internal;
using RepositoryAction = Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsCreatePullRequestsV2Mapper : IActionToRepositoryActionMapper
{
	private readonly IAzureDevOpsPullRequestService _service;
	private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
	private readonly ILogger _logger;

	public ActionAzureDevOpsCreatePullRequestsV2Mapper(IAzureDevOpsPullRequestService service, IRepositoryExpressionEvaluator expressionEvaluator, ILogger logger)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public bool CanMap(RepositoryAction action)
	{
		return action is RepositoryActionAzureDevOpsCreatePullRequestsV2;
	}

	public bool CanHandleMultipleRepositories()
	{
		return false;
	}

	public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
	{
		return Map(action as RepositoryActionAzureDevOpsCreatePullRequestsV2, repository.First());
	}

	private IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionAzureDevOpsCreatePullRequestsV2? action, Repository repository)
	{
		if (action == null)
		{
			return Array.Empty<Api.Git.RepositoryAction>();
		}

		if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
		{
			return Array.Empty<Api.Git.RepositoryAction>();
		}

		if (repository.HasLocalChanges || repository.CurrentBranch.Equals(action.ToBranch, StringComparison.OrdinalIgnoreCase))
		{
			return Array.Empty<Api.Git.RepositoryAction>();
		}

		if (string.IsNullOrWhiteSpace(action.ProjectId))
		{
			return Array.Empty<Api.Git.RepositoryAction>();
		}

		var projectId = _expressionEvaluator.EvaluateStringExpression(action.ProjectId, repository);

		if (string.IsNullOrWhiteSpace(projectId))
		{
			return Array.Empty<Api.Git.RepositoryAction>();
		}

		if (!string.IsNullOrWhiteSpace(action.ToBranch))
		{
			// check if branch exists!
			if (!repository.Branches.Contains(action.ToBranch))
			{
                _logger.LogInformation("Branch {branch} does not exist", action.ToBranch);
				return Array.Empty<Api.Git.RepositoryAction>();
			}
		}
		else
		{
		   return Array.Empty<Api.Git.RepositoryAction>();
		}

        bool hasAutoComplete = action.AutoComplete != null;
        bool isDraft = _expressionEvaluator.EvaluateBooleanExpression(action.IsDraft, repository); // not oke
        bool includeWorkItems = _expressionEvaluator.EvaluateBooleanExpression(action.IncludeWorkItems, repository); // not oke
        bool openInBrowser = _expressionEvaluator.EvaluateBooleanExpression(action.OpenInBrowser, repository); // not oke

		return new List<Api.Git.RepositoryAction>()
		{
			new(action.Name ?? $"Create Pull Request {(hasAutoComplete ? "(with auto-complete)" : string.Empty)}", repository)
			{
				Action = new DelegateAction((_, _) =>
				{
					if (hasAutoComplete)
					{
						_service.CreatePullRequestWithAutoCompleteAsync(repository, projectId, action.ReviewerIds, action.ToBranch, (int)action.AutoComplete.MergeStrategy, action.PrTitle, isDraft, includeWorkItems, openInBrowser, action.AutoComplete.DeleteSourceBranch, action.AutoComplete.TransitionWorkItems).GetAwaiter().GetResult();
					}
					else
					{
						_service.CreatePullRequestAsync(repository, projectId, action.ReviewerIds, action.ToBranch, action.PrTitle, isDraft, isDraft, openInBrowser).GetAwaiter().GetResult();
					}
				}),
			},
		};
	}
}