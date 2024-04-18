namespace RepoM.App.Services;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IUserMenuActionMenuFactory
{
    IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository);
}