namespace RepoM.Api.Git;

using System;

public interface IRepositoryObserver : IDisposable
{
    void Setup(Repository repository, TimeSpan detectionToAlertDelay);

    void Start();

    void Stop();

    Action<Repository> OnChange { get; set; }
}