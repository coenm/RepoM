namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class Text : EvaluateObjectBase
{
    public string DefaultValue { get; set; } = string.Empty;

    public static implicit operator Text(string content)
    {
        return new Text { Value = content, };
    }

    public override string ToString()
    {
        return $"{nameof(Text)} {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<string> RenderAsync(ITemplateEvaluator instance)
    {
        return await instance.RenderStringAsync(Value).ConfigureAwait(false);
    }
}