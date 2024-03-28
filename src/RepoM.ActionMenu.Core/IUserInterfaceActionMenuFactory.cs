namespace RepoM.ActionMenu.Core;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IUserInterfaceActionMenuFactory
{
    IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository, string filename);

    Task<IEnumerable<string>> GetTagsAsync(IRepository repository, string filename);
}

public static class UserInterfaceActionMenuFactoryExtensions
{
    public static async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuListAsync(this IUserInterfaceActionMenuFactory instance, IRepository repository, string filename)
    {
        var result = new List<UserInterfaceRepositoryActionBase>();
        
        await foreach (UserInterfaceRepositoryActionBase item in instance.CreateMenuAsync(repository, filename).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }
}