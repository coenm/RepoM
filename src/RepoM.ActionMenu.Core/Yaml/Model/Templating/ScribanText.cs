namespace RepoM.ActionMenu.Core.Yaml.Model.Templating;

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;

internal class ScribanText : Text, ICreateTemplate
{
    private Template? _template;
    private string? _fixedValue = null;

    void ICreateTemplate.CreateTemplate(ITemplateParser templateParser)
    {
        string value;
        if (string.IsNullOrEmpty(Value) && !string.IsNullOrWhiteSpace(DefaultValue))
        {
            value = DefaultValue;
        }
        else
        {
            value = Value;
        }

        _template ??= templateParser.ParseMixed(value);
        if (!ContainsScribanStart(value))
        {
            _template = null; // don't need _template anymore as we use fixed value.
            _fixedValue = value;
        }
    }

    public override async Task<string> RenderAsync(ITemplateEvaluator instance)
    {
        if (_fixedValue != null)
        {
            return _fixedValue;
        }
        
        if (instance is TemplateContext tc && _template != null)
        {
            return await _template.RenderAsync(tc).ConfigureAwait(false);
        }

        return await base.RenderAsync(instance).ConfigureAwait(false);
    }

    /// <summary>
    /// Stupid method to check if the value contains a scriban start tag. Depends on the fact that we know a scriban start tag is "{{".
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>true when input contains '{{'</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsScribanStart(string value)
    {
        return value?.Contains("{{") ?? false;
    }
}