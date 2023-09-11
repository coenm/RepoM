namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = Data.RepositoryAction;

public class ActionExecutableV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IFileSystem _fileSystem;

    public ActionExecutableV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, IFileSystem fileSystem)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionExecutableV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionExecutableV1, repository);
    }

    private IEnumerable<RepositoryActions.RepositoryAction> Map(RepositoryActionExecutableV1? action, IRepository repository)
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

            yield return new RepositoryActions.RepositoryAction(name, repository)
                {
                    Action = new DelegateAction((_, _) => ProcessHelper.StartProcess(normalized, arguments)),
                };
            found = true;
        }
    }
}