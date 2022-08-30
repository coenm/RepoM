namespace RepoM.Plugin.IpcService;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api;
using RepoM.Core.Plugin;

[UsedImplicitly]
internal class IpcServerModule : IModule, IDisposable
{
    private readonly IpcServer _server;

    public IpcServerModule(IpcServer server)
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