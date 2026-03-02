namespace RepoM.Core.Repositories.Reading;

using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Repositories.Model;

public interface IRepositoryInfoReader
{
    Task<RepositoryInfo?> ReadAsync(string path, CancellationToken ct = default);
}
