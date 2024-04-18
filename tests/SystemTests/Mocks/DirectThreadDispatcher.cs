namespace SystemTests.Mocks;

using System;
using System.Threading;
using RepoM.Api.Common;

internal class DirectThreadDispatcher : IThreadDispatcher
{
    public DirectThreadDispatcher()
    {
        SynchronizationContext = SynchronizationContext.Current!;
    }

    public SynchronizationContext SynchronizationContext { get; }

    public void Invoke(Action act)
    {
        act();
    }
}