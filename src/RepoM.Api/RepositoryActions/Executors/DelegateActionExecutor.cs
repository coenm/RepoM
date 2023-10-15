namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class DelegateActionExecutor : IActionExecutor<DelegateRepositoryCommand>
{
    public void Execute(IRepository repository, DelegateRepositoryCommand repositoryCommand)
    {
        repositoryCommand.Action.Invoke(null, null);
    }
}