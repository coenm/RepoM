namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// ContextAction to render a variable using a template. After rendering, the outcome is always of type string.
/// </summary>
/// <example>
/// <code>
/// - type: render-variable@1
///   name: myRenderedVariable
///   value: 'this is the current time: {{ date.now }}'
/// </code>
/// </example>
public class ContextActionRenderVariableV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "render-variable@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Value of the variable.
    /// </summary>
    [Render]
    public RenderString Value { get; init; } = new ScribanRenderString { Value = string.Empty, };

    //// <inheritdoc cref="IEnabled.Enabled"/>
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate? Enabled { get; init; } = new ScribanPredicate(); // todo nullable?

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