namespace RepoM.Core.Repositories.Persistence;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Repositories.Model;

/// <summary>
/// Persists the set of discovered repositories so the list can be shown immediately at startup,
/// before the (slower) filesystem scan and git status reads complete.
/// </summary>
public interface IRepositorySnapshotStore
{
    Task<IReadOnlyList<RepositoryInfo>> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(IEnumerable<RepositoryInfo> repositories, CancellationToken ct = default);
}
