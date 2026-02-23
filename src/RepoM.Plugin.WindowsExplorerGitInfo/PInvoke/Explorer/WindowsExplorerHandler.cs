namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

using RepoM.Core.Repositories.Store;

internal class WindowsExplorerHandler : IWindowsExplorerHandler
{
    private readonly IRepositoryStore _repositoryStore;

    public WindowsExplorerHandler(IRepositoryStore repositoryStore)
    {
        _repositoryStore = repositoryStore;
    }

    public void UpdateTitles()
    {
        var actor = new AppendRepositoryStatusTitleActor(_repositoryStore);
        actor.Pulse();
    }

    public void CleanTitles()
    {
        var actor = new CleanWindowTitleActor();
        actor.Pulse();
    }
}
