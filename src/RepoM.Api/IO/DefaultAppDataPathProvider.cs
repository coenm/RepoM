namespace RepoM.Api.IO;

using System;
using System.IO;
using RepoM.Core.Plugin.Common;

public class DefaultAppDataPathProvider : IAppDataPathProvider
{ 
    private static readonly string _applicationDataRepoM = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoM");

    private DefaultAppDataPathProvider()
    {
    }

    public static DefaultAppDataPathProvider Instance { get; } = new();

    public string AppDataPath => _applicationDataRepoM;
}