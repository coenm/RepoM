namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;

public class StatisticsService
{
    private readonly IClock _clock;
    private readonly ReadOnlyCollection<DateTime> _empty = new List<DateTime>(0).AsReadOnly();
    private readonly Dictionary<string, List<DateTime>> _recordings = new();

    public StatisticsService(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Record(IRepository repository)
    {
        if (!_recordings.TryGetValue(repository.SafePath, out List<DateTime>? list))
        {
            list = new List<DateTime>();
            _recordings.TryAdd(repository.SafePath, list);
        }

        list.Add(_clock.Now);
    }

    public IReadOnlyList<DateTime> GetRecordings(IRepository repository)
    {
        if (_recordings.TryGetValue(repository.SafePath, out List<DateTime>? list))
        {
            return list.AsReadOnly();
        }

        return _empty;
    } 
}