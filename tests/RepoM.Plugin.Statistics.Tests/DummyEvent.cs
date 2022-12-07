namespace RepoM.Plugin.Statistics.Tests;

using System;
using RepoM.Plugin.Statistics.Interface;

internal class DummyEvent : IEvent
{
    public string Repository { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}