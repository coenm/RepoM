namespace RepoM.Plugin.IpcService;

using System;
using System.Threading.Tasks;
using RepoM.Api;

internal class RepozIpcServerModule : IModule, IDisposable
{
    private readonly IpcServer _server;

    public RepozIpcServerModule(IpcServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    public Task StartAsync()
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _server.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}