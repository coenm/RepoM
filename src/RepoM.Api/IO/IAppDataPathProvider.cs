namespace RepoM.Api.IO;

using System;

public interface IAppDataPathProvider
{
    string GetAppDataPath();

    [Obsolete("Not used.")] // todo
    string GetAppResourcesPath();
}