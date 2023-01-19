namespace RepoM.Plugin.WindowsExplorerGitInfo;

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

[UsedImplicitly] 
internal class WindowExplorerBarGitInfoModule : IModule
{
    private static readonly int _delayBetweenTitleUpdatesInMs = 500;
    private Timer _explorerUpdateTimer;
    private readonly IWindowsExplorerHandler _explorerHandler;

    public WindowExplorerBarGitInfoModule(IWindowsExplorerHandler explorerHandler)
    {
        _explorerHandler = explorerHandler ?? throw new ArgumentNullException(nameof(explorerHandler));
        _explorerUpdateTimer = new Timer(RefreshTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartAsync()
    {
        _explorerUpdateTimer.Change(1000, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        Timer originalTimer = _explorerUpdateTimer;
        _explorerUpdateTimer = new Timer(_ => {}, null, Timeout.Infinite, Timeout.Infinite);
        originalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        await Task.Delay(_delayBetweenTitleUpdatesInMs + 1000);
        originalTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _explorerHandler.CleanTitles();
    }

    private void RefreshTimerCallback(object? state)
    {
        _explorerHandler.UpdateTitles();
        _explorerUpdateTimer.Change(_delayBetweenTitleUpdatesInMs, Timeout.Infinite);
    }
}