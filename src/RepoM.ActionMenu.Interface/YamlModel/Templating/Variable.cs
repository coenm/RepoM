namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class Variable : EvaluateObjectBase
{
    public object DefaultValue { get; set; } = null!;

    public static implicit operator Variable(string content)
    {
        return new Variable { Value = content, };
    }

    public override string ToString()
    {
        return $"{nameof(Variable)} {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<object> EvaluateAsync(ITemplateEvaluator instance)
    {
        return await instance.EvaluateAsync(Value).ConfigureAwait(false);
    }
}