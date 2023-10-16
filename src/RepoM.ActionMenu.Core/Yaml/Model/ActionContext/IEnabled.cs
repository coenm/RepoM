namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface IEnabled
{
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    Predicate? Enabled { get; } // nullable?
}