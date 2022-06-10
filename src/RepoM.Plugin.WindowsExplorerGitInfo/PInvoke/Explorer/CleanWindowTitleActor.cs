namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

using System;

internal class CleanWindowTitleActor : ExplorerWindowActor
{
    protected override void Act(IntPtr hwnd, string? explorerLocationUrl)
    {
        Console.WriteLine("Clean " + explorerLocationUrl ?? string.Empty);
        const string SEPARATOR = "  [";
        WindowHelper.RemoveAppendedWindowText(hwnd, SEPARATOR);
    }
}