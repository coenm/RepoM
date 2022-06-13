namespace RepoM.Api.Git;

public interface IRepositoryDetectorFactory
{
    IRepositoryDetector Create();
}