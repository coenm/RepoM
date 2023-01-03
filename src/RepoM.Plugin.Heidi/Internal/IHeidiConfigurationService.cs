namespace RepoM.Plugin.Heidi.Internal;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;

internal interface IHeidiConfigurationService
{
    Task InitializeAsync();

    IEnumerable<HeidiConfiguration> GetByRepository(IRepository repository);

    IEnumerable<HeidiConfiguration> GetByKey(string key);
}