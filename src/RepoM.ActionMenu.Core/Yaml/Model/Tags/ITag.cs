namespace RepoM.ActionMenu.Core.Yaml.Model.Tags;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface ITag
{
    string Tag { get; set;  }

    EvaluateBoolean When { get; set; }
}