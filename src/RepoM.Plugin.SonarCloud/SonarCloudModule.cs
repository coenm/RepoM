namespace RepoM.Plugin.SonarCloud;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api;
using SonarQube.Net.Common.Authentication;
using SonarQube.Net;

[UsedImplicitly]
internal class SonarCloudModule : IModule
{
    private readonly SonarCloudFavoriteService _service;

    public SonarCloudModule(SonarCloudFavoriteService service)
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