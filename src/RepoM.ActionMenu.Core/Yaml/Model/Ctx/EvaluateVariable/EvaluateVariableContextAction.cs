namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.EvaluateVariable;

using RepoM.ActionMenu.Interface.YamlModel;

public class EvaluateVariableContextAction : NamedContextAction, IContextAction, IEnabled
{
    public const string TypeValue = "evaluate-variable@1";

    public override string Type
    {
        get => TypeValue;
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

        return $"({TypeValue}) {Name} : {value}";
    }
}