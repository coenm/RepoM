namespace Grr.History;

using RepoM.Ipc;

public class State
{
    public Repository[]? LastRepositories { get; set; }

    public bool OverwriteRepositories { get; set; }

    public string? LastLocation { get; set; }
}