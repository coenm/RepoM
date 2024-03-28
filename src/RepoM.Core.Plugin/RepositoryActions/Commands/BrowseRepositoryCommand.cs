namespace RepoM.Core.Plugin.RepositoryActions.Commands;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class BrowseRepositoryCommand : IRepositoryCommand
{
    public BrowseRepositoryCommand(string url)
    {
        Url = url;
    }

    public string Url { get; }
}