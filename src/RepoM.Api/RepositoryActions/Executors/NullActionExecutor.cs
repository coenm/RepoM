namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

[UsedImplicitly]
public class NullActionExecutor : IActionExecutor<NullAction>
{
    public void Execute(IRepository repository, NullAction action)
    {
        // intentionally do nothing.
    }
}