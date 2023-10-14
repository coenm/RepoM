namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.RendererVariable;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

public class RenderVariableContextAction : NamedContextAction, IContextAction, IEnabled
{
    public const string TypeValue = "render-variable@1";

    public override string Type
    {
        get => TypeValue;
        set => _ = value;
    }

    [RenderToString("coen")]
    public RenderString Value { get; set; } = new() { Value = string.Empty};

    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Value.Value;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"{base.ToString()} : {value}";
    }
}