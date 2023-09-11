namespace RepoM.Api.RepositoryActions;

using RepoM.Core.Plugin.Repository;

public class RepositoryAction : RepositoryActionBase
{
    public RepositoryAction(string name, IRepository repository):
        base(repository)
    {
        Name = name;
    }

    public string Name { get; }
}