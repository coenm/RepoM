namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx;

using RepoM.ActionMenu.Interface.YamlModel;

public interface INamedContextAction : IContextAction
{
    string Name { get; }
}