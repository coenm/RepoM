namespace RepoM.Api.Git;

using System;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;

public interface IRepositoryMonitor
{
    event EventHandler<IRepository> OnChangeDetected;

    event EventHandler<string> OnDeletionDetected;

    event EventHandler<bool> OnScanStateChanged;

    void Stop();

    void Observe();

    void Reset();

    Task ScanForLocalRepositoriesAsync();

    void IgnoreByPath(string path);

    void SetPinned(bool newValue, IRepository repository);

    bool IsPinned(IRepository repository);
}