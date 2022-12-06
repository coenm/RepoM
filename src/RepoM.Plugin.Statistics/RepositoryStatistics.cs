namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Statistics.Interface;

internal class RepositoryStatistics
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

    public void Apply(IEvent evt)
    {
        if (evt is RepositoryActionRecordedEvent repositoryActionRecordedEvent)
        {
            Apply(repositoryActionRecordedEvent);
            return;
        }

        throw new NotImplementedException();
    }

    private void Apply(RepositoryActionRecordedEvent evt)
    {
        Recordings.Add(evt.Timestamp);
    }
}