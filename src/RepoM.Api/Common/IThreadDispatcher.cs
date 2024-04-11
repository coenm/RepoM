namespace RepoM.Api.Common;

using System;
using System.Threading;

public interface IThreadDispatcher
{
    void Invoke(Action act);

    SynchronizationContext SynchronizationContext { get; }
}