namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// ContextAction to set a variable as defined.
/// </summary>
/// <example>
/// <code>
/// - type: set-variable@1
///   name: myIntegerVariable
///   value: 123
/// </code>
/// </example>
/// <example>
/// <code>
/// - type: set-variable@1
///   name: myObjectVariable
///   value:
///   - name: Coen
///     github: https://github.com/coenm
/// </code>
/// </example>
public class ContextActionSetVariableV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "set-variable@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Value of the variable.
    /// </summary>
    public object? Value { get; init; }

    //// <inheritdoc cref="IEnabled.Enabled"/>
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public EvaluateBoolean? Enabled { get; init; } = new(); // todo nullable?

    public override string ToString()
    {
        var value = Value?.ToString() ?? "null";
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }

    public static ContextActionSetVariableV1 Create(string name, object? value)
    {
        return new ContextActionSetVariableV1()
            {
                Name = name,
                Value = value,
            };
    }
}