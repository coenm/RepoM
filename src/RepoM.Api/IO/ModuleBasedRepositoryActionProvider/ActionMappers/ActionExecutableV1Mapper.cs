namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionExecutableV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IFileSystem _fileSystem;

    public ActionExecutableV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IFileSystem fileSystem)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionExecutableV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionExecutableV1, repository.First());
    }

    private IEnumerable<RepoM.Api.Git.RepositoryAction> Map(RepositoryActionExecutableV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        // var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);

        var found = false;
        foreach (var executable in action.Executables)
        {
            if (found)
            {
                continue;
            }

            var normalized = _expressionEvaluator.EvaluateStringExpression(executable, repository);
            normalized = normalized.Replace("\"", "");

            if (!_fileSystem.File.Exists(normalized) && !_fileSystem.Directory.Exists(normalized))
            {
                continue;
            }

            var arguments = string.Empty;

            if (action.Arguments is not null)
            {
                arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments, repository);
            }

            yield return new Git.RepositoryAction(name)
                {
                    Action = new DelegateAction((_, _) => ProcessHelper.StartProcess(normalized, arguments))
                };
            found = true;
        }
    }
}