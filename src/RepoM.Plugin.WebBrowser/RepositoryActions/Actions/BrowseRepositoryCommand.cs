namespace RepoM.Plugin.WebBrowser.RepositoryActions.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class BrowseRepositoryCommand : IRepositoryCommand
{
    public BrowseRepositoryCommand(string url, string? profileName)
    {
        Url = url;
        ProfileName = profileName;
    }

    public string Url { get; }

    public string? ProfileName { get; }
}