namespace RepoM.ActionMenu.Interface.Commands;

public sealed class NullRepositoryCommand : IRepositoryCommand
{
    private NullRepositoryCommand()
    {
    }

    public static NullRepositoryCommand Instance { get; } = new();
}