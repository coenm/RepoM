namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitPullV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;

    public ActionGitPullV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitPullV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        yield return new Git.RepositoryAction(_translationService.Translate("Pull"), repository)
            {
                Action = new DelegateAction((_, _) =>
                    {
                        try
                        {
                            _repositoryWriter.Pull(repository);
                        }
                        catch
                        {
                            // nothing to see here
                        }
                    }),
                ExecutionCausesSynchronizing = true,
            };
    }
}