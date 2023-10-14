namespace RepoM.ActionMenu.Core.Yaml.Model.Tags;

using System.ComponentModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal class TagObject : ITag
{
    public string Tag { get; set; }

    [EvaluateToBoolean(true)]
    [DefaultValue(true)] // todo
    public EvaluateBoolean When { get; set; } = new EvaluateBoolean();
}