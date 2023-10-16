namespace RepoM.ActionMenu.Core.Yaml.Model.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;

internal class ScribanEvaluateBoolean : EvaluateBoolean, ICreateTemplate
{
    private Template? _template;

    void ICreateTemplate.CreateTemplate(ITemplateParser templateParser)
    {
        _template ??= templateParser.ParseScriptOnly(Value);
    }

    public override async Task<bool> EvaluateAsync(ITemplateEvaluator instance)
    {
        if (instance is TemplateContext tc && _template != null)
        {
            var result = await _template.EvaluateAsync(tc).ConfigureAwait(false);
            return ToBool(result);
        }

        return await base.EvaluateAsync(instance).ConfigureAwait(false);
    }
}