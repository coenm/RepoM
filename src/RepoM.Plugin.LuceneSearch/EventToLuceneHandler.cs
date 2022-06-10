namespace RepoM.Plugin.LuceneSearch;

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api;
using RepoM.Api.Git;

[UsedImplicitly]
internal class EventToLuceneHandler : IModule, IDisposable
{
    private readonly IRepositoryMonitor _monitor;
    private readonly IRepositoryIndex _index;
    private readonly IRepositorySearch _search;

    public EventToLuceneHandler(IRepositoryMonitor monitor, IRepositoryIndex index, IRepositorySearch search)
    {
        _monitor = monitor;
        _index = index;
        _search = search;
    }

    public Task StartAsync()
    {
        _monitor.OnChangeDetected += MonitorOnOnChangeDetected;
        _monitor.OnDeletionDetected += MonitorOnOnDeletionDetected;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        Unregister();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Unregister();
    }

    private void Unregister()
    {
        _monitor.OnChangeDetected -= MonitorOnOnChangeDetected;
        _monitor.OnDeletionDetected -= MonitorOnOnDeletionDetected;
    }

    private void MonitorOnOnDeletionDetected(object? sender, string path)
    {
        _index.RemoveFromIndexByPath(path);
        _index.FlushAndCommit();
        (_search as SearchAdapter)?.ResetCache();
    }

    private void MonitorOnOnChangeDetected(object? sender, Repository e)
    {
        var repo = new RepositorySearchModel(
            e.Name,
            e.Path,
            e.Tags.ToList());

        _index.ReIndexMediaFileAsync(repo);
        (_search as SearchAdapter)?.ResetCache();
    }
}