namespace RepoM.Plugin.Demo;

using System;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

/// <summary>Simulates repository commands without constructing or invoking the real executor.</summary>
public sealed class DemoCommandExecutorDecorator<TCommand> : ICommandExecutor<TCommand>
    where TCommand : IRepositoryCommand
{
    private readonly ILogger _logger;

    public DemoCommandExecutorDecorator(Func<ICommandExecutor<TCommand>> decoratee, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Execute(IRepository repository, TCommand action)
    {
        _logger.LogInformation("Demo: simulated {Command} for {Repository}; no external action was performed.",
            typeof(TCommand).Name, repository.Name);
    }
}
