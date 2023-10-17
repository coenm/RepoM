namespace RepoM.ActionMenu.Core;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IUserInterfaceActionMenuFactory
{
    Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync(IRepository repository, string filename);

    Task<IEnumerable<string>> GetTagsAsync(IRepository repository, string filename);
}