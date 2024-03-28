namespace RepoM.Core.Plugin.RepositoryActions.Commands;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class NullRepositoryCommand : IRepositoryCommand
{
    private NullRepositoryCommand()
    {
    }

    public static NullRepositoryCommand Instance { get; } = new NullRepositoryCommand();
}