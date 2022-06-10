namespace RepoM.Api.Common;

using System;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}