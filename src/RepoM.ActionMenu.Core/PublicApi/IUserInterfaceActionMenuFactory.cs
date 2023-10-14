namespace RepoM.ActionMenu.Core.PublicApi;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IUserInterfaceActionMenuFactory
{
    Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync(IRepository repository);
    
    Task<IEnumerable<string>> GetTagsAsync(IRepository repository);
}