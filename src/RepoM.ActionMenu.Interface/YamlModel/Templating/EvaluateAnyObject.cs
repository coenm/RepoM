namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class ScriptContent : EvaluateAnyObject
{
    public static implicit operator ScriptContent(string content)
    {
        return new ScriptContent { Value = content, };
    }
}

public class EvaluateAnyObject : EvaluateObject
{
    public object DefaultValue { get; set; } = null!;

    public static implicit operator EvaluateAnyObject(string content)
    {
        return new EvaluateAnyObject { Value = content, };
    }

    public override string ToString()
    {
        return $"EvaluateAnyObject {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<object> EvaluateAsync(ITemplateEvaluator instance)
    {
        return await instance.EvaluateAsync(Value).ConfigureAwait(false);
    }
}