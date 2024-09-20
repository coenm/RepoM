namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.AzureDevOps.Internal;

[UsedImplicitly]
internal class AzureDevOpsModule : IModule
{
    private readonly IAzureDevOpsPullRequestService _service;

    public AzureDevOpsModule(IAzureDevOpsPullRequestService service)
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