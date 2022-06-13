namespace RepoM.Api.IO;

public interface IPathSkipper
{
    bool ShouldSkip(string path);
}