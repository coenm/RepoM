namespace RepoM.Core.Plugin.Common;

using System;

/// <summary>
/// Provides RepoM common directories.
/// </summary>
public interface IAppDataPathProvider
{
    string AppDataPath { get; }

    [Obsolete("Not used.")] // todo
    string GetAppResourcesPath();
}