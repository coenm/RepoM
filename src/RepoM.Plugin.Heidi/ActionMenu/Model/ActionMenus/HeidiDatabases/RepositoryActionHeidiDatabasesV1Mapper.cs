namespace RepoM.Plugin.Heidi.ActionMenu.Model.ActionMenus.HeidiDatabases;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;

[UsedImplicitly]
internal class RepositoryActionHeidiDatabasesV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionHeidiDatabasesV1>
{
    private readonly IHeidiConfigurationService _service;
    private readonly IHeidiSettings _settings;
    private readonly ILogger _logger;

    public RepositoryActionHeidiDatabasesV1Mapper(
        IHeidiConfigurationService service,
        IHeidiSettings settings,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionHeidiDatabasesV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var executable = await GetHeidiExecutable(action, context).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(executable))
        {
            _logger.LogWarning("HeidiSQL executable was empty");
            yield break;
        }

        string? key = null;
        if (action.Key != null)
        {
            key =  await action.Key.RenderAsync(context).ConfigureAwait(false);
        }
        
        RepositoryHeidiConfiguration[] databases = GetHeidiDatabases(repository, key);

        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(name))
        {
            if (databases.Length > 0)
            {
                yield return new UserInterfaceRepositoryAction(name, repository)
                    {
                        CanExecute = true,
                        SubActions = GetDbActions(repository, databases, executable),
                    };
            }
            else
            {
                yield return new UserInterfaceRepositoryAction(name, repository)
                    {
                        CanExecute = true,
                        SubActions = new[]
                            {
                                new UserInterfaceRepositoryAction("NO databases found!", repository)
                                    {
                                        RepositoryCommand = NullRepositoryCommand.Instance,
                                        CanExecute = false,
                                    },
                            },
                    };
            }

            yield break;
        }

        if (databases.Length > 0)
        {
            foreach (UserInterfaceRepositoryActionBase repositoryAction in GetDbActions(repository, databases, executable))
            {
                yield return repositoryAction;
            }
        }
        else
        {
            yield return new UserInterfaceRepositoryAction("NO databases found!", repository)
                {
                    RepositoryCommand = NullRepositoryCommand.Instance,
                    CanExecute = false,
                };
        }
    }

    private async Task<string> GetHeidiExecutable(RepositoryActionHeidiDatabasesV1 action, ITemplateEvaluator context)
    {
        var executable = await action.Executable.RenderAsync(context).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(executable) ? _settings.DefaultExe : executable;
    }

    private static IEnumerable<UserInterfaceRepositoryActionBase> GetDbActions(IRepository repository, IEnumerable<RepositoryHeidiConfiguration> databases, string executable)
    {
        return databases.Select(database => new UserInterfaceRepositoryAction(database.Name, repository)
            {
                RepositoryCommand = new StartProcessRepositoryCommand(executable, new[] { "--description", $"\"{database.DbConfig.Key}\"", }),
                ExecutionCausesSynchronizing = false,
            });
    }

    private RepositoryHeidiConfiguration[] GetHeidiDatabases(IRepository repository, string? key)
    {
        return (string.IsNullOrWhiteSpace(key)
                ? _service.GetByRepository(repository)
                : _service.GetByKey(key))
            .ToArray();
    }
}