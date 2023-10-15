namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.ExecuteScript;

using RepoM.ActionMenu.Interface.YamlModel;

public sealed class ContextActionExecuteScriptV1 : IContextAction, IEnabled
{
    public const string TYPE_VALUE = "evaluate-script@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string Content { get; init; }

    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Content;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) : {value}";
    }
}