namespace RepoM.Api.Git;

using System.Threading.Tasks;

public interface IRepositoryReader
{
    Task<Repository?> ReadRepositoryAsync(string path);
}