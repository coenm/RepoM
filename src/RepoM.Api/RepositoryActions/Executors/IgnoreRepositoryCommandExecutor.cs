namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class IgnoreRepositoryCommandExecutor : ICommandExecutor<IgnoreRepositoryCommand>
{
    private readonly IRepositoryMonitor _repositoryMonitor;

    public IgnoreRepositoryCommandExecutor(IRepositoryMonitor repositoryMonitor)
    {
        _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
    }

    public void Execute(IRepository repository, IgnoreRepositoryCommand repositoryCommand)
    {
        try
        {
            _repositoryMonitor.IgnoreByPath(repository.Path);
        }
        catch
        {
            // nothing to see here
        }
    }
}