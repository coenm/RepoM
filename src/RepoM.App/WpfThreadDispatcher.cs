namespace RepoM.App;

using System;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;

internal class WpfThreadDispatcher : IThreadDispatcher
{
    private readonly ILogger _logger;

    public WpfThreadDispatcher(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SynchronizationContext = SynchronizationContext.Current!;
    }

    public void Invoke(Action act)
    {
        _logger.LogDebug("Start invoke on UI thread");
        Application.Current.Dispatcher.Invoke(act);
        _logger.LogDebug("Stop invoke on UI thread");
    }

    public SynchronizationContext SynchronizationContext { get; }
}