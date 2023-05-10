namespace RepoM.Plugin.AzureDevOps.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using RepoM.Plugin.AzureDevOps.Internal;
using RepositoryAction = Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsCreatePullRequestsV1Mapper : IActionToRepositoryActionMapper
{
	private readonly IAzureDevOpsPullRequestService _service;
	private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
	private readonly ILogger _logger;

	public ActionAzureDevOpsCreatePullRequestsV1Mapper(IAzureDevOpsPullRequestService service, IRepositoryExpressionEvaluator expressionEvaluator, ILogger logger)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public bool CanMap(RepositoryAction action)
	{
		return action is RepositoryActionAzureDevOpsCreatePullRequestsV1;
	}

	public bool CanHandleMultipleRepositories()
	{
		return false;
	}

	public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, IEnumerable<Api.Git.Repository> repository, ActionMapperComposition actionMapperComposition)
	{
		return Map(action as RepositoryActionAzureDevOpsCreatePullRequestsV1, repository.First());
	}

	private IEnumerable<Api.Git.RepositoryAction> Map(RepositoryActionAzureDevOpsCreatePullRequestsV1? action, Api.Git.Repository repository)
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
				return Array.Empty<Api.Git.RepositoryAction>();
			}
		}
		else
		{
		   return Array.Empty<Api.Git.RepositoryAction>();
		}

		return new List<Api.Git.RepositoryAction>()
		{
			new(action.Title ?? $"Create Pull Request {(action.AutoComplete.Enabled ? "(with auto-complete)" : string.Empty)}", repository)
			{
				Action = new DelegateAction((_, _) =>
				{
					if (action.AutoComplete.Enabled)
					{
						_service.CreatePullRequestWithAutoCompleteAsync(repository, projectId, action.ReviewerIds, action.ToBranch, (int)action.AutoComplete.MergeStrategy, action.PrTitle, action.DraftPr, action.IncludeWorkItems, action.OpenInBrowser, action.AutoComplete.DeleteSourceBranch, action.AutoComplete.TransitionWorkItems).GetAwaiter().GetResult();
					}
					else
					{
						_service.CreatePullRequestAsync(repository, projectId, action.ReviewerIds, action.ToBranch, action.PrTitle, action.DraftPr, action.IncludeWorkItems, action.OpenInBrowser).GetAwaiter().GetResult();
					}
				}),
			},
		};
	}
}