namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.LoadFile;

using RepoM.ActionMenu.Interface.YamlModel;

public class LoadFileContextAction : NamedContextAction, IContextAction, IEnabled
{
    public const string TypeValue = "load-file@1";

    public override string Type
    {
        get => TypeValue;
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

        return $"({TypeValue}) {Name} : {value}";
    }
}