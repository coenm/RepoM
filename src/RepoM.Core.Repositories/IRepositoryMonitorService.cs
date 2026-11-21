namespace RepoM.Core.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Plugin;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;

/// <summary>
/// Coordinates repository discovery, monitoring and refreshes, including module startup and shutdown.
/// Consumers and module registrations must resolve the same instance so decorators cover both paths.
/// </summary>
public interface IRepositoryMonitorService : IModule, IDisposable
{
    /// <summary>Gets the store of known repositories.</summary>
    IRepositoryStore Store { get; }

    /// <summary>Observes whether repository discovery is running.</summary>
    IObservable<bool> IsScanning { get; }

    /// <summary>Gets whether a stale-repository check is running.</summary>
    bool IsStalenessCheckRunning { get; }

    /// <summary>Enables monitoring for the repository identified by its normalized path.</summary>
    void ActivateMonitoring(string safePath);

    /// <summary>Disables monitoring for the repository identified by its normalized path.</summary>
    void DeactivateMonitoring(string safePath);

    /// <summary>Cancels active repository discovery scans.</summary>
    void CancelAllScans();

    /// <summary>Scans the configured locations for repositories.</summary>
    Task ScanAsync(CancellationToken ct = default);

    /// <summary>Removes repositories whose directories no longer exist.</summary>
    void RemoveStaleRepositories();

    /// <summary>Refreshes the status of all monitored repositories.</summary>
    Task RefreshAllAsync(CancellationToken ct = default);

    /// <summary>Refreshes one repository and returns its updated information, if available.</summary>
    Task<RepositoryInfo?> RefreshRepositoryAsync(string repositoryPath, CancellationToken ct = default);
}
