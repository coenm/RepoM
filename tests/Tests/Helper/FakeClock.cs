namespace Tests.Helper;

using System;
using RepoM.Api.Common;

internal class FakeClock : IClock
{
    public FakeClock(DateTime fakeValue)
    {
        Now = fakeValue;
    }

    public DateTime Now { get; }
}