namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

using RepoM.ActionMenu.Interface.YamlModel;

public interface INamedContextAction : IContextAction
{
    string Name { get; }
}