namespace RepoM.ActionMenu.Interface.YamlModel;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public class DefaultMenuAction : IMenuAction
{
    public string Type { get; }

    public Predicate Active { get; }

    public DefaultMenuAction()
    {
        Type = string.Empty;
        Active = new Predicate();
    }
}