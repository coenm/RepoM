namespace RepoM.ActionMenu.Core.Yaml.Model.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;

internal class ScribanEvaluateInt : EvaluateInt, ICreateTemplate
{
    private Template? _template = null;

    void ICreateTemplate.CreateTemplate(ITemplateParser templateParser)
    {
        _template ??= templateParser.ParseScriptOnly(Value);
    }

    public override async Task<int> EvaluateAsync(ITemplateEvaluator instance)
    {
        if (instance is not TemplateContext tc || _template == null)
        {
            return await base.EvaluateAsync(instance).ConfigureAwait(false);
        }

        var result = await _template.EvaluateAsync(tc).ConfigureAwait(false);
        if (result is null)
        {
            return DefaultValue;
        }
            
        if (result is int i)
        {
            return i;
        }

        if (result is string s && int.TryParse(s, out int r))
        {
            return r;
        }

        return DefaultValue;
    }
}