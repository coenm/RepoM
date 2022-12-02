namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

[UsedImplicitly]
public class BrowseActionExecutor : IActionExecutor<BrowseAction>
{
    public void Execute(IRepository repository, BrowseAction action)
    {
        throw new NotImplementedException();
    }
}