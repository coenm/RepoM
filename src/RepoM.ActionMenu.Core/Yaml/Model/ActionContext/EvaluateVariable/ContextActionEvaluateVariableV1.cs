namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// ContextAction to evaluate a template and capture and store the result as variable.
/// </summary>
/// <example>
/// <code>
/// - type: evaluate-variable@1
///   name: currentTimeVariable
///   value: 'date.now'
/// </code>
/// </example>
public class ContextActionEvaluateVariableV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "evaluate-variable@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Value of the variable.
    /// </summary>
    [EvaluateToAnyObject]
    public string? Value { get; init; }

    /// <inheritdoc cref="IEnabled.Enabled"/>
    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Value;
        if (value?.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}