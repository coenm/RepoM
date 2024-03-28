namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class StartProcessCommandExecutor : ICommandExecutor<StartProcessRepositoryCommand>
{
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

        ProcessHelper.StartProcess(repositoryCommand.Executable, args);
    }
}