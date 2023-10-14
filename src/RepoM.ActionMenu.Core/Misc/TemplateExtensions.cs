// ReSharper disable once CheckNamespace, Justification: Extension methods
namespace Scriban;

using System;

internal static class TemplateExtensions
{
    public static void ThrowOnError(this Template template)
    {
        if (template.HasErrors)
        {
            throw new Exception("Template has errors");
        }

        // todo
        // string.Join(',', template.Messages)
    }
}