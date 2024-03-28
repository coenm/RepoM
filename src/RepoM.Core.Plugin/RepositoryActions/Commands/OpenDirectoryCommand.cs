namespace RepoM.Core.Plugin.RepositoryActions.Commands;

public sealed class OpenDirectoryCommand : IRepositoryCommand
{
    public OpenDirectoryCommand(string directoryPath)
    {
        Path = directoryPath;
    }

    public string Path { get; }
}