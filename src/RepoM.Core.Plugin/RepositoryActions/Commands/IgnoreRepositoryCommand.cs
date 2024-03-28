namespace RepoM.Core.Plugin.RepositoryActions.Commands;

public sealed class IgnoreRepositoryCommand : IRepositoryCommand
{
    private IgnoreRepositoryCommand()
    {
    }

    public static IgnoreRepositoryCommand Instance { get; } = new();
}