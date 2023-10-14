namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

public abstract class EvaluateObject
{
    public string Value { get; set; }

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