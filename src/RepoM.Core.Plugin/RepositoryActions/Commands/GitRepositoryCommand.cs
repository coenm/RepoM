namespace RepoM.Core.Plugin.RepositoryActions.Commands;

public sealed class GitRepositoryCommand : IRepositoryCommand
{
    private GitRepositoryCommand(GitActionType action, string? branch = null)
    {
        GitAction = action;
        Branch = branch ?? string.Empty;
    }

    public GitActionType GitAction { get; }

    public string Branch { get; }

    public static GitRepositoryCommand Fetch { get; } = new(GitActionType.Fetch);

    public static GitRepositoryCommand Pull { get; } = new(GitActionType.Pull);

    public static GitRepositoryCommand Push { get; } = new(GitActionType.Push);

    public static GitRepositoryCommand Checkout(string branch)
    {
        return new GitRepositoryCommand(GitActionType.Checkout, branch);
    }

    public enum GitActionType
    {
        Fetch,

        Pull,

        Push,

        Checkout,
    }
}