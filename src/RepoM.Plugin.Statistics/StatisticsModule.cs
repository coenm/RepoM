namespace RepoM.Plugin.Statistics;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;

[UsedImplicitly]
internal class StatisticsModule : IModule
{
    private readonly StatisticsService _service;

    public StatisticsModule(StatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
        // return _service.InitializeAsync();
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}