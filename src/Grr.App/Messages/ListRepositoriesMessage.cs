namespace Grr.App.Messages;

using RepoM.Ipc;

[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
public class ListRepositoriesMessage : IMessage
{
    private readonly string _repositoryFilter;

    public ListRepositoriesMessage()
    {
        _repositoryFilter = string.Empty;
    }

    public ListRepositoriesMessage(RepositoryFilterOptions filter)
    {
        _repositoryFilter = filter?.RepositoryFilter ?? string.Empty;
    }

    public void Execute(Repository[] repositories)
    {
        // nothing to do
    }

    public string GetRemoteCommand()
    {
        return string.IsNullOrEmpty(_repositoryFilter)
            ? "list:.*" /* show all with RegEx pattern ".*" */
            : $"list:{RegexFilter.Get(_repositoryFilter)}";
    }

    public bool HasRemoteCommand => true;

    public bool ShouldWriteRepositories(Repository[] repositories)
    {
        return true;
    }
}