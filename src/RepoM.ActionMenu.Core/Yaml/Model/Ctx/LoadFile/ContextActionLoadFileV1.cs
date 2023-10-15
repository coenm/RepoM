namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.LoadFile;

using RepoM.ActionMenu.Interface.YamlModel;

public class ContextActionLoadFileV1 : NamedContextAction, IContextAction, IEnabled
{
    public const string TYPE_VALUE = "load-file@1";

    public override string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string? Filename { get; init; }

    public string? Enabled { get; init; }

    public override string ToString()
    {
        var value = Filename;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"({TYPE_VALUE}) {Name} : {value}";
    }
}