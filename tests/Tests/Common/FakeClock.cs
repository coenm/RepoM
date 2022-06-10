namespace Tests.Common;

using RepoZ.Api.Common;
using System;
using RepoM.Api.Common;

public class FakeClock : IClock
{
    public FakeClock(DateTime fakeValue)
    {
        FakeValue = fakeValue;
    }

    public DateTime Now => FakeValue;

    public DateTime FakeValue { get; }
}