namespace RepoM.Plugin.Heidi.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions;
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
    private readonly IHeidiSettings _settings;
    private readonly ILogger _logger;
    
    public ActionHeidiDatabasesV1Mapper(
        IHeidiConfigurationService service,
        IRepositoryExpressionEvaluator expressionEvaluator,
        IHeidiSettings settings,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanMap(RepositoryAction action)
    {
        return action is RepositoryActionHeidiDatabasesV1;
    }

    public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionHeidiDatabasesV1, repository);
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionHeidiDatabasesV1? action, IRepository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var executable = GetHeidiExecutable(action, repository);
        if (string.IsNullOrWhiteSpace(executable))
        {
            _logger.LogWarning("HeidiSQL executable was empty");
            yield break;
        }

        RepositoryHeidiConfiguration[] databases = GetHeidiDatabases(action, repository);

        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);

        if (!string.IsNullOrWhiteSpace(name))
        {
            if (databases.Length > 0)
            {
                yield return new Api.RepositoryActions.RepositoryAction(name, repository)
                    {
                        CanExecute = true,
                        SubActions = GetDbActions(repository, databases, executable),
                    };
            }
            else
            {
                yield return new Api.RepositoryActions.RepositoryAction(name, repository)
                    {
                        CanExecute = true,
                        SubActions = new[]
                            {
                                new Api.RepositoryActions.RepositoryAction("NO databases found!", repository)
                                    {
                                        Action = NullAction.Instance,
                                        CanExecute = false,
                                    },
                            },
                    };
            }
  
            yield break;
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
            yield return new Api.RepositoryActions.RepositoryAction("NO databases found!", repository)
                {
                    Action = NullAction.Instance,
                    CanExecute = false,
                };
        }
    }

    private RepositoryHeidiConfiguration[] GetHeidiDatabases(RepositoryActionHeidiDatabasesV1 action, IRepository repository)
    {
        return (string.IsNullOrWhiteSpace(action.Key)
                ? _service.GetByRepository(repository)
                : _service.GetByKey(action.Key))
            .ToArray();
    }

    private string GetHeidiExecutable(RepositoryActionHeidiDatabasesV1 action, IRepository repository)
    {
        return string.IsNullOrWhiteSpace(action.Executable)
            ? _settings.DefaultExe
            : _expressionEvaluator.EvaluateStringExpression(action.Executable, repository);
    }

    private static IEnumerable<RepositoryActionBase> GetDbActions(IRepository repository, IEnumerable<RepositoryHeidiConfiguration> databases, string executable)
    {
        return databases.Select(database => new Api.RepositoryActions.RepositoryAction(database.Name, repository)
            {
               Action = new StartProcessAction(executable, new[] { "--description" , $"\"{database.DbConfig.Key}\"", }),
               ExecutionCausesSynchronizing = false,
            });
    }
}