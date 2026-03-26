namespace RepoM.Core.Repositories.Monitoring;

using System;

public interface IRepositoryMonitoringEvents
{
    event Action<string, bool>? MonitoringChanged;
}
