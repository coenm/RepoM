namespace RepoM.Api.Common;

using System;
using RepoM.Core.Plugin.Common;

public class SystemClock : IClock
{
    private SystemClock()
    {
    }

    public static SystemClock Instance { get; } = new();

    public DateTime Now => DateTime.Now;
}