namespace RepoM.Plugin.Heidi;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.Heidi.Internal;

[UsedImplicitly]
internal class HeidiModule : IModule
{
    private readonly IHeidiConfigurationService _service;

    public HeidiModule(IHeidiConfigurationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task StartAsync()
    {
        return _service.InitializeAsync();
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}