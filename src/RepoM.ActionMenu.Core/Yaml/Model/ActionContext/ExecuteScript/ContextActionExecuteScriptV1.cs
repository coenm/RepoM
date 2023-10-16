namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;


/// <summary>
/// ContextAction to evaluate a template as script. The result is not stored but all variables and functions defined in the script can be accessed during the next steps.
/// </summary>
/// <example>
/// <code>
/// - type: evaluate-script@1
///   content: |-
///     name = 'coenm';
///     b = 'beer';
///     w = 'wine';
///     name = name + ' drinks a lot of ' + b + ' and ' + w;
///             
///     now2 = date.now;
///             
///     func sub1
///       ret $0 - $1
///     end
///     
///     func sub2(x, y)
///       ret x - y + 10
///     end
///     
///     func sonar_url(project_id)
///       ret 'https://sonarcloud.io/project/overview?id='  + project_id;
///     end
///     
///     dummy_calc = sub2(19, 3);
/// </code>
/// </example>
public sealed class ContextActionExecuteScriptV1 : IContextAction, IEnabled
{
    public const string TYPE_VALUE = "evaluate-script@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Script content.
    /// </summary>
    [Script]
    public ScriptContent? Content { get; init; } = new (); // todo nullable ?

    //// <inheritdoc cref="IEnabled.Enabled"/>
    /// <summary>
    /// Whether the variable is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public EvaluateBoolean? Enabled { get; init; } = new(); // todo nullable?

    public override string ToString()
    {
        var value = Content?.Value;
        if (value?.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) : {value}";
    }
}