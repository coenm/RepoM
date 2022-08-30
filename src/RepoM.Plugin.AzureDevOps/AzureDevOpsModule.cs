namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api;
using RepoM.Core.Plugin;

[UsedImplicitly]
internal class AzureDevOpsModule : IModule
{
    private readonly AzureDevOpsPullRequestService _service;

    public AzureDevOpsModule(AzureDevOpsPullRequestService service)
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