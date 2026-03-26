namespace RepoM.Core.Repositories.Monitoring;

using System;
using System.Collections.Concurrent;

public sealed class RepositoryMonitoringStateService : IRepositoryMonitoringService, IRepositoryMonitoringEvents
{
    private readonly ConcurrentDictionary<string, bool> _monitoredRepositories = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string, bool>? MonitoringChanged;

    public bool IsMonitored(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);
        return _monitoredRepositories.TryGetValue(safePath, out var monitored) && monitored;
    }

    public void SetMonitored(string safePath, bool monitored)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);

        if (monitored)
        {
            if (_monitoredRepositories.TryGetValue(safePath, out var existing) && existing)
            {
                return;
            }

            _monitoredRepositories[safePath] = true;
        }
        else
        {
            if (!_monitoredRepositories.TryGetValue(safePath, out var existing) || !existing)
            {
                return;
            }

            _monitoredRepositories.TryRemove(safePath, out _);
        }

        MonitoringChanged?.Invoke(safePath, monitored);
    }

    public void EnableMonitoring(string safePath)
    {
        SetMonitored(safePath, true);
    }
}
