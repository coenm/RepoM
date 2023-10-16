namespace RepoM.ActionMenu.Core.Yaml.Model.Tags;

using System.ComponentModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <inheritdoc cref="ITag"/>
internal class TagObject : ITag
{
    /// <inheritdoc cref="ITag.Tag"/>
    public string Tag { get; set; } = string.Empty;

    /// <inheritdoc cref="ITag.When"/>
    [EvaluateToBoolean(true)]
    [DefaultValue(true)] // todo
    public Predicate When { get; set; } = new Predicate();
}