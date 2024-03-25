namespace RepoM.Api.Git;

using System;
using RepoM.Core.Plugin.Repository;

public interface IRepositoryObserver : IDisposable
{
    void Setup(IRepository repository, int detectionToAlertDelayMilliseconds);

    void Start();

    void Stop();

    Action<IRepository> OnChange { get; set; }
}