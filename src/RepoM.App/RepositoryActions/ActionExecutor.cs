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

    /// <summary>
    /// Executes a command related to the repository.
    /// </summary>
    /// <param name="repository">The repository</param>
    /// <param name="repositoryCommand">The command.</param>
    /// <returns><c>true</c> when the command was executed successfully, <c>false</c> otherwise.</returns>
    public bool Execute(IRepository repository, IRepositoryCommand repositoryCommand)
    {
        try
        {
            dynamic executor = _container.GetInstance(typeof(ICommandExecutor<>).MakeGenericType(repositoryCommand.GetType()));
            executor.Execute((dynamic)repository, (dynamic)repositoryCommand);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Execute command '{Command}' failed.", repositoryCommand.GetType().Name);
            return false;
        }
    }
}