namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public class OpenDirectoryCommandExecutor : ICommandExecutor<OpenDirectoryCommand>
{
    public void Execute(IRepository repository, OpenDirectoryCommand repositoryCommand)
    {
        ProcessHelper.StartProcess(repositoryCommand.Path, string.Empty);
    }
}