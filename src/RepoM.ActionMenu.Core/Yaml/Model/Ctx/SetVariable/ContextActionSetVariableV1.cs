namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.SetVariable;

using RepoM.ActionMenu.Interface.YamlModel;

public class ContextActionSetVariableV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "set-variable@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public object? Value { get; init; }

    public string? Enabled { get; init; }

    public static ContextActionSetVariableV1 Create(string name, object? value)
    {
        return new ContextActionSetVariableV1()
        {
            Name = name,
            Value = value,
        };
    }

    public override string ToString()
    {
        var value = Value?.ToString() ?? "null";
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}