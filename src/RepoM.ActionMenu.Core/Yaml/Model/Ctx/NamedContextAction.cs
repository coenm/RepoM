namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx;

public abstract class NamedContextAction : INamedContextAction
{
    public abstract string Type { get; set; }

    public string Name { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"({Type}) {Name}";
    }
}