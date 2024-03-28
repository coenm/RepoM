namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

public abstract class NamedContextAction : INamedContextAction
{
    // No xml comment to keep the generated yaml clean
    public abstract string Type { get; set; }

    /// <summary>
    /// Variable name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"({Type}) {Name}";
    }
}