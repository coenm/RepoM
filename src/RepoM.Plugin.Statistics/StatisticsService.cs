namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.Interface;

public class StatisticsService
{
    private readonly IClock _clock;
    private readonly ReadOnlyCollection<DateTime> _empty = new List<DateTime>(0).AsReadOnly();
    private readonly ConcurrentDictionary<string, RepositoryStatistics> _recordings = new();
    private readonly Subject<IEvent> _events;

    public StatisticsService(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _events = new Subject<IEvent>();
    }

    public IObservable<IEvent> Events => _events;

    public void Record(IRepository repository)
    {
        if (!_recordings.TryGetValue(repository.SafePath, out RepositoryStatistics? repositoryStatistics))
        {
            repositoryStatistics = new RepositoryStatistics(repository.SafePath, _clock);
            _recordings.TryAdd(repository.SafePath, repositoryStatistics);
        }

        IEvent evt = repositoryStatistics.Record();
        _events.OnNext(evt);
    }

    public IReadOnlyList<string> GetRepositories()
    {
        return _recordings.Select(x => x.Key).ToImmutableArray();
    }

    internal IReadOnlyRepositoryStatistics? GetRepositoryRecording(IRepository repository)
    {
        return _recordings.TryGetValue(repository.SafePath, out RepositoryStatistics? repositoryStatistics)
            ? repositoryStatistics
            : null;
    }

    public IReadOnlyList<DateTime> GetRecordings(IRepository repository)
    {
        if (_recordings.TryGetValue(repository.SafePath, out RepositoryStatistics? repositoryStatistics))
        {
            return repositoryStatistics.Recordings.AsReadOnly();
        }

        return _empty;
    }

    public void Apply(IEvent evt)
    {
        if (!_recordings.TryGetValue(evt.Repository, out RepositoryStatistics? repositoryStatistics))
        {
            repositoryStatistics = new RepositoryStatistics(evt.Repository, _clock);
            _recordings.TryAdd(evt.Repository, repositoryStatistics);
        }

        repositoryStatistics.Apply(evt);
    }
}