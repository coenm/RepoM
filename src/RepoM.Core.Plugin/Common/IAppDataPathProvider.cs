namespace RepoM.Core.Plugin.Common;

public interface IAppDataPathProvider
{
    string AppDataPath { get; }

    string AppResourcesPath { get; }
}