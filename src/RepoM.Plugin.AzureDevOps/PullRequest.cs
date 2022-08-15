namespace RepoM.Plugin.AzureDevOps;

internal class PullRequest
{
    public PullRequest(string name, string url)
    {
        Name = name;
        Url = url;
    }

    public string Name { get; }

    public string Url { get; }
}