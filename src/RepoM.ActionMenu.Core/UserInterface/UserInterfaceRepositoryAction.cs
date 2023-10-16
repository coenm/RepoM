namespace RepoM.ActionMenu.Core.UserInterface;

using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public class UserInterfaceRepositoryAction : UserInterfaceRepositoryActionBase
{
    public UserInterfaceRepositoryAction(string name, IRepository repository) :
        base(repository)
    {
        Name = name;
    }

    public string Name { get; }
}