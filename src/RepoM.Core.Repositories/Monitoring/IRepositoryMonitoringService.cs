namespace RepoM.Core.Repositories.Monitoring;

public interface IRepositoryMonitoringService
{
    bool IsMonitored(string safePath);

    void SetMonitored(string safePath, bool monitored);

    void EnableMonitoring(string safePath);
}
