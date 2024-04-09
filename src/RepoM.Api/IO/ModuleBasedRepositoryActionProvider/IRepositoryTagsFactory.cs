namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Api.Git;

public interface IRepositoryTagsFactory
{
    Task<IEnumerable<string>> GetTagsAsync(Repository repository);
}