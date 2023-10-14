namespace RepoM.ActionMenu.Core.Yaml.Model.Templating;

using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;

internal class ScribanRenderString : RenderString, ICreateTemplate
{
    private Template? _template;

    void ICreateTemplate.CreateTemplate(ITemplateParser templateParser)
    {
        _template ??= templateParser.ParseMixed(Value);
    }

    public override async Task<string> RenderAsync(ITemplateEvaluator instance)
    {
        if (instance is TemplateContext tc && _template != null)
        {
            return await _template.RenderAsync(tc).ConfigureAwait(false);
        }

        return await base.RenderAsync(instance).ConfigureAwait(false);
    }
}