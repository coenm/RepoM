namespace RepoM.Plugin.Statistics.Interface;

using System;

internal class RepositoryActionRecordedEvent : IEvent
{
    public string Repository { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}