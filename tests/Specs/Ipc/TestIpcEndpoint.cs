namespace Specs.Ipc;

using RepoM.Ipc;

class TestIpcEndpoint : IIpcEndpoint
{
    public string Address => "inproc://abc";
    // public string Address => "tcp://localhost:18182";
}