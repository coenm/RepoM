namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface IEnabled
{
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [Predicate(true)]
    Predicate Enabled { get; } // nullable?
}