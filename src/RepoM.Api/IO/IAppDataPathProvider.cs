namespace RepoM.Api.IO;

public interface IAppDataPathProvider
{
    string GetAppDataPath();

    string GetAppResourcesPath();
}