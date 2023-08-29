namespace RepoM.Plugin.Heidi.ActionProvider;

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
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionHeidiDatabasesV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IHeidiConfigurationService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IHeidiSettings _settings;
    private readonly ILogger _logger;
    
    public ActionHeidiDatabasesV1Mapper(
        IHeidiConfigurationService service,
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IHeidiSettings settings,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanMap(RepositoryAction action)
    {
        return action is RepositoryActionHeidiDatabasesV1;
    }

    public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionHeidiDatabasesV1, repository.First());
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionHeidiDatabasesV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var executable = string.IsNullOrWhiteSpace(action.Executable)
            ? _settings.DefaultExe
            : _expressionEvaluator.EvaluateStringExpression(action.Executable, repository);

        if (string.IsNullOrWhiteSpace(executable))
        {
            _logger.LogWarning("HeidiSQL executable was empty");
            yield break;
        }

        RepositoryHeidiConfiguration[] databases = (string.IsNullOrWhiteSpace(action.Key)
                ? _service.GetByRepository(repository)
                : _service.GetByKey(action.Key))
            .ToArray();

        string? name = action.Name;

        if (!string.IsNullOrWhiteSpace(name))
        {
            name = NameHelper.EvaluateName(name, repository, _translationService, _expressionEvaluator);

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (databases.Length > 0)
                {
                    yield return new Api.Git.RepositoryAction(name, repository)
                        {
                            CanExecute = true,
                            SubActions = GetDbActions(repository, databases, executable),
                        };
                }
                else
                {
                    yield return new Api.Git.RepositoryAction(name, repository)
                        {
                            CanExecute = true,
                            SubActions = new[]
                                {
                                    new Api.Git.RepositoryAction("NO databases found!", repository)
                                        {
                                            Action = NullAction.Instance,
                                            CanExecute = false,
                                        },
                                },
                        };
                }
      
                yield break;
            }
        }

        if (databases.Length > 0)
        {
            foreach (RepositoryActionBase repositoryAction in GetDbActions(repository, databases, executable))
            {
                yield return repositoryAction;
            }
        }
        else
        {
            yield return new Api.Git.RepositoryAction("NO databases found!", repository)
                {
                    Action = NullAction.Instance,
                    CanExecute = false,
                };
        }
    }

    private static IEnumerable<RepositoryActionBase> GetDbActions(IRepository repository, IEnumerable<RepositoryHeidiConfiguration> databases, string executable)
    {
        return databases.Select(database => new Api.Git.RepositoryAction(database.Name, repository)
            {
               Action = new StartProcessAction(executable, new[] { "--description" , $"\"{database.DbConfig.Key}\"", }),
               ExecutionCausesSynchronizing = false,
            });
    }
}