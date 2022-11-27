namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RepoM.Core.Plugin;

public class StatisticsService
{
    private readonly ReadOnlyCollection<DateTime> _empty = new List<DateTime>(0).AsReadOnly();
    private readonly Dictionary<string, List<DateTime>> _recordings = new();

    public void Record(IRepository repository)
    {
        if (!_recordings.TryGetValue(repository.SafePath, out List<DateTime>? list))
        {
            list = new List<DateTime>();
            _recordings.TryAdd(repository.SafePath, list);
        }

        list.Add(DateTime.Now);
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