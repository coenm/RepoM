namespace RepoM.Ipc.Tests.Internals;

using RepoM.Ipc;

internal class TestIpcEndpoint : IIpcEndpoint
{
    public string Address => "tcp://localhost:18182";
}