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
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;

    public IgnoreRepositoryCommandExecutor(IRepositoryIgnoreStore repositoryIgnoreStore)
    {
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
    }

    public void Execute(IRepository repository, IgnoreRepositoryCommand repositoryCommand)
    {
        try
        {
            _repositoryIgnoreStore.IgnoreByPath(repository.Path);
        }
        catch
        {
            // nothing to see here
        }
    }
}
