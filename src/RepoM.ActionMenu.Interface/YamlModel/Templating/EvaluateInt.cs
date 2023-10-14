namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class EvaluateInt : EvaluateObject
{
    public int DefaultValue { get; set; }

    public static implicit operator EvaluateInt(string content)
    {
        return new EvaluateInt { Value = content };
    }

    public override string ToString()
    {
        return $"EvaluateInt {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<int> EvaluateAsync(ITemplateEvaluator instance)
    {
        var result = await instance.EvaluateAsync(Value).ConfigureAwait(false);

        if (result == null)
        {
            return DefaultValue;
        }

        if (result is int i)
        {
            return i;
        }

        return DefaultValue;
    }
}