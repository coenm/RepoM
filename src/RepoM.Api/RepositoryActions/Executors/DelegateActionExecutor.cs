namespace RepoM.Api.RepositoryActions.Executors;

using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public class DelegateActionExecutor : IActionExecutor<DelegateAction>
{
    public void Execute(IRepository repository, DelegateAction action)
    {
        action.Action.Invoke(null, null);
    }
}