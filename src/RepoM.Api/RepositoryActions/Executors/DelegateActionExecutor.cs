namespace RepoM.Api.RepositoryActions.Executors;

using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public class DelegateActionExecutor : IActionExecutor<DelegateAction>
{
    private DelegateActionExecutor()
    {
    }

    public static DelegateActionExecutor Instance { get; } = new DelegateActionExecutor();


    public void Execute(DelegateAction action)
    {
        action.Action.Invoke(null, null);
    }
}