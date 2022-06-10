namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke;

using System;

public class WindowPath
{
    public WindowPath(IntPtr handle, string path)
    {
        Handle = handle;
        Path = path;
    }

    public IntPtr Handle { get; }

    public string Path { get; }
}