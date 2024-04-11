namespace RepoM.App;

using System;
using System.Threading;
using System.Windows;
using RepoM.Api.Common;

internal class WpfThreadDispatcher : IThreadDispatcher
{
    public WpfThreadDispatcher()
    {
        SynchronizationContext = SynchronizationContext.Current!;
    }

    public void Invoke(Action act)
    {
        Application.Current.Dispatcher.Invoke(act);
    }

    public SynchronizationContext SynchronizationContext { get; }
}