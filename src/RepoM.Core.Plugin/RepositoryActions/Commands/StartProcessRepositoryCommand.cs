namespace RepoM.Core.Plugin.RepositoryActions.Commands;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class StartProcessRepositoryCommand : IRepositoryCommand
{
    public StartProcessRepositoryCommand(string executable, params string[] arguments)
    {
        Executable = executable;
        Arguments = arguments;
    }

    public string Executable { get; }

    public string[] Arguments { get; }
}