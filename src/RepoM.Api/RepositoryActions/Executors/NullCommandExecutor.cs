namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class NullCommandExecutor : ICommandExecutor<NullRepositoryCommand>
{
    public void Execute(IRepository repository, NullRepositoryCommand repositoryCommand)
    {
        // intentionally do nothing.
    }
}