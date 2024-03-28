namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

public abstract class EvaluateObjectBase
{
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        var value = Value;
        if (value.Length > 10)
        {
            value = value[..10] + "..";
        }

        return $"{value}";
    }
}