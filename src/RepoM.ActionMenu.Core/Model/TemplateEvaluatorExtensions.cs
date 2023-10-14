namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal static class TemplateEvaluatorExtensions
{
    public static Task<string> RenderStringAsync(this ITemplateEvaluator instance, RenderString renderString)
    {
        return renderString.RenderAsync(instance);
    }

    public static Task<string> TranslateAsync(this ITemplateEvaluator instance, string text)
    {
        // todo method
        return instance.RenderStringAsync("{{ translate('" + text + ")}}");
    }

    public static async Task<string> RenderNullableString(this ITemplateEvaluator instance, string? text)
    {
        if (text == null)
        {
            return string.Empty;
        }

        return await instance.RenderStringAsync(text).ConfigureAwait(false);
    }

    public static async Task<bool> EvaluateToBooleanAsync(this ITemplateEvaluator instance, string? text, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return defaultValue;
        }

        var result = await instance.EvaluateAsync(text);

        if (result == null)
        {
            // log
            return defaultValue;
        }

        if (result is bool b)
        {
            return b;
        }

        if (result is 0)
        {
            return false;
        }

        if (result is int)
        {
            return true;
        }

        if (result is "true")
        {
            return true;
        }

        if (result is "false")
        {
            return false;
        }

        throw new Exception();
    }
}