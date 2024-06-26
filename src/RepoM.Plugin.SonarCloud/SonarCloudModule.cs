namespace RepoM.Plugin.SonarCloud;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;

[UsedImplicitly]
internal class SonarCloudModule : IModule
{
    private readonly ISonarCloudFavoriteService _service;

    public SonarCloudModule(ISonarCloudFavoriteService service)
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