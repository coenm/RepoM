namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common.Common;
using RepoM.Api.Common.IO.ExpressionEvaluator;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.Git;
using RepositoryAction = RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitCheckoutV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;

    public ActionGitCheckoutV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitCheckoutV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionGitCheckoutV1, repository.First());
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionGitCheckoutV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = action.Name;
        if (!string.IsNullOrEmpty(name))
        {
            name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = _translationService.Translate("Checkout");
        }

        yield return new RepoM.Api.Git.RepositoryAction(_translationService.Translate("Checkout"))
            {
                DeferredSubActionsEnumerator = () =>
                    repository.LocalBranches
                              .Take(50)
                              .Select(branch => new RepoM.Api.Git.RepositoryAction(branch)
                                  {
                                      Action = (_, _) => _repositoryWriter.Checkout(repository, branch),
                                      CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                  })
                              .Union(new RepositoryActionBase[]
                                  {
                                      new RepositorySeparatorAction(), // doesn't work todo
                                      new RepoM.Api.Git.RepositoryAction(_translationService.Translate("Remote branches"))
                                          {
                                              DeferredSubActionsEnumerator = () =>
                                                  {
                                                      RepoM.Api.Git.RepositoryAction[] remoteBranches = repository.ReadAllBranches()
                                                                                                                  .Select(branch => new RepoM.Api.Git.RepositoryAction(branch)
                                                                                                                      {
                                                                                                                          Action = (_, _) => _repositoryWriter.Checkout(repository, branch),
                                                                                                                          CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                                                      })
                                                                                                                  .ToArray();

                                                      if (remoteBranches.Any())
                                                      {
                                                          return remoteBranches;
                                                      }

                                                      return new RepositoryActionBase[]
                                                          {
                                                              new RepoM.Api.Git.RepositoryAction(_translationService.Translate("No remote branches found"))
                                                                  {
                                                                      CanExecute = false,
                                                                  },
                                                              new RepoM.Api.Git.RepositoryAction(_translationService.Translate("Try to fetch changes if you're expecting remote branches"))
                                                                  {
                                                                      CanExecute = false,
                                                                  },
                                                          };
                                                  },
                                          },
                                  })
                              .ToArray(),
            };
    }
}