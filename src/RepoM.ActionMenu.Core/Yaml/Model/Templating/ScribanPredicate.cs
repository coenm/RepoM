namespace RepoM.ActionMenu.Core.Yaml.Model.Templating;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;

internal class ScribanPredicate : Predicate, ICreateTemplate
{
    private bool? _fixedValue;
    private Template? _template;

    void ICreateTemplate.CreateTemplate(ITemplateParser templateParser)
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            _fixedValue = DefaultValue;
        }

        _template ??= templateParser.ParseScriptOnly(Value);
    }

    public override async Task<bool> EvaluateAsync(ITemplateEvaluator instance)
    {
        if (StaticValue.HasValue)
        {
            return StaticValue.Value;
        }

        if (_fixedValue.HasValue)
        {
            return _fixedValue.Value;
        }

        if (instance is TemplateContext tc && _template != null)
        {
            try
            {
                var result = await _template.EvaluateAsync(tc).ConfigureAwait(false);
                return ToBool(result);
            }
            catch (Exception e)
            {
                throw new PredicateEvaluationException(Value, e);
            }
        }

        return await base.EvaluateAsync(instance).ConfigureAwait(false);
    }
}