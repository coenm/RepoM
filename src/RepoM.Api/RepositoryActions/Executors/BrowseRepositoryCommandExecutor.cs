namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class BrowseRepositoryCommandExecutor : ICommandExecutor<BrowseRepositoryCommand>
{
    public void Execute(IRepository repository, BrowseRepositoryCommand action)
    {
        // untested.
        ProcessHelper.StartProcess(action.Url, string.Empty);
    }
}