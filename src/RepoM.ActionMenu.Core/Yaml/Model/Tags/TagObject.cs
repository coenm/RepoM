namespace RepoM.ActionMenu.Core.Yaml.Model.Tags;

using JetBrains.Annotations;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <inheritdoc cref="ITag"/>
[UsedImplicitly]
internal class TagObject : ITag
{
    /// <inheritdoc cref="ITag.Tag"/>
    public string Tag { get; set; } = string.Empty;

    /// <inheritdoc cref="ITag.When"/>
    [Predicate(true)]
    public Predicate When { get; set; } = new ScribanPredicate();
}