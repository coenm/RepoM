namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class OpenDirectoryCommandExecutor : ICommandExecutor<OpenDirectoryCommand>
{
    private readonly ILogger _logger;

    public OpenDirectoryCommandExecutor(ILogger logger)
    {
        _logger = logger;
    }

    public void Execute(IRepository repository, OpenDirectoryCommand repositoryCommand)
    {
        ProcessHelper.StartProcess(repositoryCommand.Path, string.Empty, _logger);
    }
}