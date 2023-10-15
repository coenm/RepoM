namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// ContextAction to load configuration from an other file.
/// </summary>
/// <example>
/// <code>
/// - type: load-file@1
///   filename: '{basedir}/my-file.yaml'
/// </code>
/// </example>
public class ContextActionLoadFileV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "load-file@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Full path of the filename to load.
    /// </summary>
    [RenderToString]
    public string? Filename { get; init; }

    /// <inheritdoc cref="IEnabled.Enabled"/>
    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Filename;
        if (value?.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}