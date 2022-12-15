namespace RepoM.Api.IO;

using System;

// todo, check original code where this was used?!
public interface IPathFinder
{
    bool CanHandle(string processName);

    string FindPath(IntPtr windowHandle);
}