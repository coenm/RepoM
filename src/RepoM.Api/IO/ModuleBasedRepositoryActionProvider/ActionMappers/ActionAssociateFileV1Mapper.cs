namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionAssociateFileV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionAssociateFileV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionAssociateFileV1;
    }

    public bool CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionAssociateFileV1, repository.First());
    }

    private IEnumerable<RepoM.Api.Git.RepositoryAction> Map(RepositoryActionAssociateFileV1? action, Repository repository)
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
        // var command = _expressionEvaluator.EvaluateStringExpression(action.Command, repository);

        //todo no arguments needed.
        // var arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments, repository);

        if (action.Extension is null)
        {
            yield break;
        }

        RepoM.Api.Git.RepositoryAction? menuItem = CreateFileAssociationSubMenu(
            repository,
            name,
            action.Extension);

        if (menuItem != null)
        {
            yield return menuItem;
        }
    }

    private static Git.RepositoryAction CreateProcessRunnerAction(string name, string process, IRepository repository, string arguments = "")
    {
        return new Git.RepositoryAction(name, repository)
        {
            Action = new DelegateAction((_, _) => ProcessHelper.StartProcess(process, arguments)),
        };
    }

    private RepoM.Api.Git.RepositoryAction? CreateFileAssociationSubMenu(Repository repository, string actionName, string filePattern)
    {
        if (!HasFiles(repository, filePattern))
        {
            return null;
        }

        return new RepoM.Api.Git.RepositoryAction(actionName, repository)
            {
                DeferredSubActionsEnumerator = () =>
                    GetFiles(repository, filePattern)
                        .Select(solutionFile => CreateProcessRunnerAction(Path.GetFileName(solutionFile), solutionFile, repository))
                        .ToArray(),
            };
    }

    private static bool HasFiles(Repository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern).Any();
    }

    private static IEnumerable<string> GetFiles(Repository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern)
               .OrderBy(f => f)
               .Take(25);
    }

    private static IEnumerable<string> GetFileEnumerator(Repository repository, string searchPattern)
    {
        // prefer EnumerateFileSystemInfos() over EnumerateFiles() to include packaged folders like
        // .app or .xcodeproj on macOS

        var directory = new DirectoryInfo(repository.Path);
        return directory
               .EnumerateFileSystemInfos(searchPattern, SearchOption.AllDirectories)
               .Select(f => f.FullName)
               .Where(f => !f.StartsWith("."));
    }
}