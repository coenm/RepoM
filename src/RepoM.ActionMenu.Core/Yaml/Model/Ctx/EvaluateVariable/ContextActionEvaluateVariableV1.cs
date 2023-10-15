namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.EvaluateVariable;

using RepoM.ActionMenu.Interface.YamlModel;

public class ContextActionEvaluateVariableV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "evaluate-variable@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string Value { get; init; }

    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Value;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}