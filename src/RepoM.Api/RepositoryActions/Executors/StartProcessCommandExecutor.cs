namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class StartProcessCommandExecutor : ICommandExecutor<StartProcessRepositoryCommand>
{
    private readonly ILogger _logger;

    public StartProcessCommandExecutor(ILogger logger)
    {
        _logger = logger;
    }

    public void Execute(IRepository repository, StartProcessRepositoryCommand repositoryCommand)
    {
        var args = string.Empty;

        if (repositoryCommand.Arguments.Length == 1)
        {
            args = repositoryCommand.Arguments[0];
        }
        else if (repositoryCommand.Arguments.Length > 1)
        {
            args = string.Join(' ', repositoryCommand.Arguments);
        }

        ProcessHelper.StartProcess(repositoryCommand.Executable, args, _logger);
    }
}