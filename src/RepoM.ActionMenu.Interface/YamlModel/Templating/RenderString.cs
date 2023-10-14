namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

public class RenderString : EvaluateObject
{
    public string DefaultValue { get; set; } = string.Empty;

    public static implicit operator RenderString(string content)
    {
        return new RenderString { Value = content };
    }

    public override string ToString()
    {
        return $"RenderString {base.ToString()} : {DefaultValue}";
    }

    public virtual async Task<string> RenderAsync(ITemplateEvaluator instance)
    {
        return await instance.RenderStringAsync(Value).ConfigureAwait(false);
    }
}