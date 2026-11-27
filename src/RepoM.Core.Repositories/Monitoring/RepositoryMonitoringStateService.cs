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

        while (true)
        {
            if (_monitoredRepositories.TryGetValue(safePath, out var existing))
            {
                if (existing == monitored)
                {
                    return;
                }

                if (!_monitoredRepositories.TryUpdate(safePath, monitored, existing))
                {
                    continue;
                }
            }
            else if (!_monitoredRepositories.TryAdd(safePath, monitored))
            {
                continue;
            }

            if (existing != monitored)
            {
                MonitoringChanged?.Invoke(safePath, monitored);
            }

            return;
        }
    }

    public void EnableMonitoring(string safePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(safePath);

        // A stored false is an explicit opt-out; automatic activation must preserve it.
        if (_monitoredRepositories.TryAdd(safePath, true))
        {
            MonitoringChanged?.Invoke(safePath, true);
        }
    }
}
