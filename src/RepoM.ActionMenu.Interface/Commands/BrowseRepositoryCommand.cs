namespace RepoM.ActionMenu.Interface.Commands;

public sealed class BrowseRepositoryCommand : IRepositoryCommand
{
    public BrowseRepositoryCommand(string url)
    {
        Url = url;
    }

    public string Url { get; }
}