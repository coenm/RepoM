namespace RepoM.ActionMenu.Core.Yaml.Model.Tags;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Repository tag.
/// </summary>
public interface ITag
{
    /// <summary>
    /// Name of the Tag.
    /// </summary>
    string Tag { get; set;  }

    /// <summary>
    /// Boolean expression to determine if the tag is enabled.
    /// </summary>
    Predicate When { get; set; }
}