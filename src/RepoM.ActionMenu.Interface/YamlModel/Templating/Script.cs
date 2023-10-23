namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

public class Script : Variable
{
    public static implicit operator Script(string content)
    {
        return new Script { Value = content, };
    }
}