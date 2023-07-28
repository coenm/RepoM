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

    [Obsolete("Not used.")]
    public string GetAppResourcesPath()
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new NotSupportedException("Could not get entry point of assembly.");
        }

        var result = Path.GetDirectoryName(entryAssembly.Location);

        if (result == null)
        {
            throw new FileNotFoundException("Could not find location of entry assembly");
        }

        return result;
    }
}