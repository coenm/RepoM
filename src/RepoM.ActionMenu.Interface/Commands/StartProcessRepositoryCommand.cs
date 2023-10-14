namespace RepoM.ActionMenu.Interface.Commands;

public sealed class StartProcessRepositoryCommand : IRepositoryCommand
{
    // ProcessHelper.StartProcess(command, arguments)
    public StartProcessRepositoryCommand(string command, string arguments)
    {
        Command = command;
        Arguments = arguments;
    }

    public string Command { get; }

    public string Arguments { get; }
}