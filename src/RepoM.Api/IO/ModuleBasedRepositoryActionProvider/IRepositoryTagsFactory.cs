namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;

public interface IRepositoryTagsFactory
{
    Task<IEnumerable<string>> GetTagsAsync(IRepository repository);
}
