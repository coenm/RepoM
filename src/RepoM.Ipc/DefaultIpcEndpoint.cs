namespace RepoM.Ipc;

public class DefaultIpcEndpoint : IIpcEndpoint
{
    public string Address => "tcp://localhost:18181";
}