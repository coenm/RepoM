namespace RepoM.Plugin.Statistics.Interface;

using System;

public interface IEvent
{
    public string Repository { get; set; }

    public DateTime Timestamp { get; set; }
}