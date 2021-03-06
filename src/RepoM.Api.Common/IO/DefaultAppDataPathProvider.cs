namespace RepoM.Api.Common.IO;

using System;
using System.IO;
using RepoM.Api.IO;

public class DefaultAppDataPathProvider : IAppDataPathProvider
{
    private static readonly string _applicationDataRepoM = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoM");

    public string GetAppDataPath() 
    {
        return _applicationDataRepoM ;
    }

    public string GetAppResourcesPath()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    }
}