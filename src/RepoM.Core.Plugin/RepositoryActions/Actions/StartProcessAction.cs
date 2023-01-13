namespace RepoM.Core.Plugin.RepositoryActions.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class StartProcessAction : IAction
{
    public StartProcessAction(string executable, string[] arguments)
    {
        Executable = executable;
        Arguments = arguments;
    }

    public string Executable { get; }

    public string[] Arguments { get; }
}