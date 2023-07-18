namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Statistics.Interface;

internal class RepositoryStatistics : IReadOnlyRepositoryStatistics
{
    private readonly string _repositoryPath;
    private readonly IClock _clock;
    
    public RepositoryStatistics(string repositoryPath, IClock clock)
    {
        _repositoryPath = repositoryPath ?? throw new ArgumentNullException(nameof(repositoryPath));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public List<DateTime> Recordings { get; } = new();

    public IEvent Record()
    {
        var evt = new RepositoryActionRecordedEvent
            {
                Repository = _repositoryPath,
                Timestamp = _clock.Now,
            };

        Apply(evt);

        return evt;
    }

    int IReadOnlyRepositoryStatistics.GetRecordingCount(DateTime from, DateTime to)
    {
        return Recordings.Count(recordingDate => recordingDate < to && recordingDate >= from);
    }

    int IReadOnlyRepositoryStatistics.GetRecordingCountFrom(DateTime from)
    {
        return Recordings.Count(recordingDate => recordingDate >= from);
    }

    int IReadOnlyRepositoryStatistics.GetRecordingCountBefore(DateTime to)
    {
        return Recordings.Count(recordingDate => recordingDate < to);
    }

    public void Apply(IEvent evt)
    {
        if (evt is RepositoryActionRecordedEvent repositoryActionRecordedEvent)
        {
            Apply(repositoryActionRecordedEvent);
            return;
        }

        throw new InvalidOperationException($"Type '{evt.GetType().Name}' is unknown");
    }

    private void Apply(RepositoryActionRecordedEvent evt)
    {
        Recordings.Add(evt.Timestamp);
    }
}