namespace RepoM.App.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IUserMenuActionMenuFactory
{
    IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository);

    /// <summary>
    /// Pre-loads and parses the repository actions file so the first context-menu open is fast.
    /// </summary>
    Task WarmupAsync();
}