namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryFinder;

public class WindowsPathSkipper : IPathSkipper
{
    private readonly List<string> _exclusions;

    public WindowsPathSkipper()
    {
        _exclusions = new List<string>()
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"$Recycle.Bin",
                @"\Package Cache",
                @"\.nuget",
                @"\Local\Temp",
            };
    }

    public bool ShouldSkip(string path)
    {
        return _exclusions.Exists(ex => path.IndexOf(ex, StringComparison.OrdinalIgnoreCase) > -1);
    }
}