namespace RepoM.Core.Plugin.Common;

using System;

public interface IAppDataPathProvider
{
    string GetAppDataPath();

    [Obsolete("Not used.")] // todo
    string GetAppResourcesPath();
}