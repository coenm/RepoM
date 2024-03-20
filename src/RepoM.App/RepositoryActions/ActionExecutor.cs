namespace RepoM.App.RepositoryActions;

using System;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using SimpleInjector;

public sealed class ActionExecutor
{
    private readonly Container _container;
    private readonly ILogger _logger;

    public ActionExecutor(Container container, ILogger logger)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Execute(IRepository repository, IRepositoryCommand repositoryCommand)
    {
        try
        {
            dynamic executor = _container.GetInstance(typeof(ICommandExecutor<>).MakeGenericType(repositoryCommand.GetType()));
            executor.Execute((dynamic)repository, (dynamic)repositoryCommand);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Execute command '{Command}' failed.", repositoryCommand.GetType().Name);
        }
    }
}