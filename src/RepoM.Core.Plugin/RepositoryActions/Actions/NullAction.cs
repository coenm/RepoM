namespace RepoM.Core.Plugin.RepositoryActions.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class NullAction : IAction
{
    private NullAction()
    {
    }

    public static NullAction Instance { get; } = new NullAction();
}