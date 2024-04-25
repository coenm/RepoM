namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class BrowseRepositoryCommandExecutor : ICommandExecutor<BrowseRepositoryCommand>
{
    private readonly ILogger _logger;

    public BrowseRepositoryCommandExecutor(ILogger logger)
    {
        _logger = logger;
    }

    public void Execute(IRepository repository, BrowseRepositoryCommand action)
    {
        // untested.
        ProcessHelper.StartProcess(action.Url, string.Empty, _logger);
    }
}