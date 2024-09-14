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

    /*
     * The purpose of this function is to execute a process specified by the
     * Executable property of the repositoryCommand object, along with
     * any additional arguments passed in the Arguments property.
     * The ProcessHelper.StartProcess method is responsible for starting
     * the process with the specified executable and arguments.
     * Overall, this function takes care of executing a process based on
     * the provided repository command, handling different scenarios
     * for the number of arguments,
     * and logging any relevant information using the _logger object.
     */
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