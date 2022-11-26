namespace RepoM.Api.RepositoryActions.Executors;

using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public class NullActionExecutor : IActionExecutor<NullAction>
{
    public void Execute(NullAction action)
    {
        return;
    }
}