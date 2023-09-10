namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = Data.RepositoryAction;

public class ActionGitCheckoutV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;

    public ActionGitCheckoutV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitCheckoutV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionGitCheckoutV1, repository);
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

        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);

        if (string.IsNullOrWhiteSpace(name))
        {
            name = _translationService.Translate("Checkout");
        }

        yield return new DeferredRepositoryAction(name, repository, false) 
            {
                DeferredSubActionsEnumerator = () =>
                    repository.LocalBranches
                              .Take(50)
                              .Select(branch => new Git.RepositoryAction(branch, repository)
                                  {
                                      Action = new DelegateAction((_, _) => _repositoryWriter.Checkout(repository, branch)),
                                      CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                  })
                              .Union(new RepositoryActionBase[]
                                  {
                                      new RepositorySeparatorAction(repository), // doesn't work todo
                                      new DeferredRepositoryAction(_translationService.Translate("Remote branches"), repository, false)
                                          {
                                              DeferredSubActionsEnumerator = () =>
                                                  {
                                                      Git.RepositoryAction[] remoteBranches = repository
                                                                                              .ReadAllBranches()
                                                                                              .Select(branch => new Git.RepositoryAction(branch, repository)
                                                                                                  {
                                                                                                      Action = new DelegateAction((_, _) => _repositoryWriter.Checkout(repository, branch)),
                                                                                                      CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                                  })
                                                                                              .ToArray();

                                                      if (remoteBranches.Any())
                                                      {
                                                          return remoteBranches;
                                                      }

                                                      return new RepositoryActionBase[]
                                                          {
                                                              new Git.RepositoryAction(_translationService.Translate("No remote branches found"), repository)
                                                                  {
                                                                      CanExecute = false,
                                                                  },
                                                              new Git.RepositoryAction(_translationService.Translate("Try to fetch changes if you're expecting remote branches"), repository)
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