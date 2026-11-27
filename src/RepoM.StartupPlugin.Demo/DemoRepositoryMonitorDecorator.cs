namespace RepoM.StartupPlugin.Demo;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Store;

/// <summary>
/// Supplies in-memory repositories for the normal UI and module lifecycle without starting Git,
/// filesystem watchers, periodic scans or the production monitor's persistence.
/// </summary>
public sealed class DemoRepositoryMonitorDecorator : IRepositoryMonitorService
{
    private readonly IRepositoryMonitoringService _monitoring;

    public DemoRepositoryMonitorDecorator(Func<IRepositoryMonitorService> decoratee,
        IRepositoryStore store, IRepositoryMonitoringService monitoring)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
        Store = store ?? throw new ArgumentNullException(nameof(store));
        _monitoring = monitoring ?? throw new ArgumentNullException(nameof(monitoring));
    }

    public IRepositoryStore Store { get; }

    public IObservable<bool> IsScanning { get; } = Observable.Return(false);

    public bool IsStalenessCheckRunning => false;

    public Task StartAsync()
    {
        _monitoring.EnableMonitoring(DemoRepositories.PrimarySafePath);
        return ScanAsync();
    }

    public Task ScanAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Store.AddOrUpdateRange(DemoRepositories.Create());
        return Task.CompletedTask;
    }

    public Task RefreshAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task<RepositoryInfo?> RefreshRepositoryAsync(string repositoryPath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(repositoryPath);
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Store.Items.SingleOrDefault(repository =>
            string.Equals(repository.Path, repositoryPath, StringComparison.OrdinalIgnoreCase)));
    }

    public void ActivateMonitoring(string safePath) => _monitoring.SetMonitored(safePath, true);

    public void DeactivateMonitoring(string safePath) => _monitoring.SetMonitored(safePath, false);

    // Fake paths must not be checked against the real filesystem.
    public void RemoveStaleRepositories() { }

    public void CancelAllScans() { }

    public Task StopAsync() => Task.CompletedTask;

    // The decoratee factory is never invoked, so this decorator owns no live monitor resources.
    public void Dispose() { }
}
