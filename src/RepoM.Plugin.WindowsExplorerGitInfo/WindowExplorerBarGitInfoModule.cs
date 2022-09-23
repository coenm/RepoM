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
    private readonly Timer _explorerUpdateTimer;
    private readonly WindowsExplorerHandler _explorerHandler;

    public WindowExplorerBarGitInfoModule(WindowsExplorerHandler explorerHandler)
    {
        _explorerHandler = explorerHandler ?? throw new ArgumentNullException(nameof(explorerHandler));
        _explorerUpdateTimer = new Timer(RefreshTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartAsync()
    {
        _explorerUpdateTimer.Change(1000, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _explorerUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _explorerHandler.CleanTitles();
        return Task.CompletedTask;
    }

    private void RefreshTimerCallback(object? state)
    {
        _explorerHandler.UpdateTitles();
        _explorerUpdateTimer.Change(500, Timeout.Infinite);
    }
}