namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public sealed class PinRepositoryCommandExecutor : ICommandExecutor<PinRepositoryCommand>
{
    private readonly IRepositoryMonitor _monitor;

    public PinRepositoryCommandExecutor(IRepositoryMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public void Execute(IRepository repository, PinRepositoryCommand repositoryCommand)
    {
        // todo
        // at this moment, we must cast to Repository

        if (repository is not Repository repo)
        {
            return;
        }

        var newPinnedValue = repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Pin;
        newPinnedValue |= repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Toggle && !_monitor.IsPinned(repo);
      _monitor.SetPinned(newPinnedValue, repo);
    }
}