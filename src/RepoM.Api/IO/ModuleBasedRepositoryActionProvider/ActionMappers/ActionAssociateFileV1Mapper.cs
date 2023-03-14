namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionAssociateFileV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionAssociateFileV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
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

    private IEnumerable<Git.RepositoryAction> Map(RepositoryActionAssociateFileV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        if (action.Extension is null)
        {
            yield break;
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        
        Git.RepositoryAction? menuItem = CreateFileAssociationSubMenu(
            repository,
            name,
            action.Extension);

        if (menuItem != null)
        {
            yield return menuItem;
        }
    }

    private static Git.RepositoryAction? CreateFileAssociationSubMenu(IRepository repository, string actionName, string filePattern)
    {
        if (!HasFiles(repository, filePattern))
        {
            return null;
        }

        return new Git.RepositoryAction(actionName, repository)
            {
                DeferredSubActionsEnumerator = () =>
                    GetFiles(repository, filePattern)
                        .Select(solutionFile => CreateProcessRunnerAction(Path.GetFileName(solutionFile), solutionFile, repository))
                        .ToArray(),
            };
    }

    private static Git.RepositoryAction CreateProcessRunnerAction(string name, string process, IRepository repository, string arguments = "")
    {
        return new Git.RepositoryAction(name, repository)
            {
                Action = new DelegateAction((_, _) => ProcessHelper.StartProcess(process, arguments)),
            };
    }

    private static bool HasFiles(IRepository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern).Any();
    }

    private static IEnumerable<string> GetFiles(IRepository repository, string searchPattern)
    {
        return GetFileEnumerator(repository, searchPattern)
               .OrderBy(f => f)
               .Take(25);
    }

    private static IEnumerable<string> GetFileEnumerator(IRepository repository, string searchPattern)
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