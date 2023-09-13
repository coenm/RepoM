namespace RepoM.Plugin.WebBrowser.RepositoryActions.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class BrowseAction : IAction
{
    public BrowseAction(string url, string? profileName)
    {
        Url = url;
        ProfileName = profileName;
    }

    public string Url { get; }

    public string? ProfileName { get; }
}