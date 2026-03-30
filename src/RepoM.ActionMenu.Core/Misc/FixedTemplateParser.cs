namespace RepoM.ActionMenu.Core.Misc;

using System;
using System.Collections.Concurrent;
using RepoM.ActionMenu.Core.Model;
using Scriban;

internal class FixedTemplateParser : ITemplateParser
{
    private static readonly ConcurrentDictionary<string, Template> _scriptOnlyCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Template> _mixedCache = new(StringComparer.Ordinal);

    public Template ParseScriptOnly(string text)
    {
        return _scriptOnlyCache.GetOrAdd(text, static t =>
        {
            var template = Template.Parse(t, sourceFilePath: null!, DefaultLexerAndParserOptions.DefaultParserOptions, DefaultLexerAndParserOptions.ScriptOnlyLexer);
            ThrowOnError(template);
            return template;
        });
    }

    public Template ParseMixed(string text)
    {
        return _mixedCache.GetOrAdd(text, static t =>
        {
            var template = Template.Parse(t, sourceFilePath: null!, DefaultLexerAndParserOptions.DefaultParserOptions, DefaultLexerAndParserOptions.MixedLexer);
            ThrowOnError(template);
            return template;
        });
    }

    private static void ThrowOnError(Template template)
    {
        if (template.HasErrors)
        {
            throw new Exception($"Template has errors {template.Messages}");
        }
    }
}