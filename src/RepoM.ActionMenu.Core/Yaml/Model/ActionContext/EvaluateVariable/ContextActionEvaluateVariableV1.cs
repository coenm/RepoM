namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
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
    [Variable]
    public Variable Value { get; init; } = new ScribanVariable();

    //// <inheritdoc cref="IEnabled.Enabled"/>
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Enabled { get; init; } = new ScribanPredicate();

    public override string ToString()
    {
        var value = Value.Value;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}