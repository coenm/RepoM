namespace RepoM.Api.Common.IO;

using System;
using System.IO;
using RepoM.Api.IO;

public class DefaultAppDataPathProvider : IAppDataPathProvider
{
    private static readonly string _applicationDataRepoZ = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoZ");

    public string GetAppDataPath() 
    {
        return _applicationDataRepoZ ;
    }

    public string GetAppResourcesPath()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    }
}