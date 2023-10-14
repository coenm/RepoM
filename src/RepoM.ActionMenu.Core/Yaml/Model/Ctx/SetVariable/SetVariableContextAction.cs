namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.SetVariable;

using RepoM.ActionMenu.Interface.YamlModel;

public class SetVariableContextAction : NamedContextAction, IContextAction, IEnabled
{
    public const string TypeValue = "set-variable@1";

    public override string Type
    {
        get => TypeValue;
        set => _ = value;
    }

    public object? Value { get; init; }

    public string? Enabled { get; init; }

    public static SetVariableContextAction Create(string name, object? value)
    {
        return new SetVariableContextAction()
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

        return $"({TypeValue}) {Name} : {value}";
    }
}