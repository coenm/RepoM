namespace RepoM.ActionMenu.Core.Misc;

using System;
using RepoM.ActionMenu.Core.Model;
using Scriban;

internal class FixedTemplateParser : ITemplateParser
{
    public Template ParseScriptOnly(string text)
    {
        var template = Template.Parse(text, sourceFilePath: null!, DefaultLexerAndParserOptions.DefaultParserOptions, DefaultLexerAndParserOptions.ScriptOnlyLexer);
        ThrowOnError(template); 
        return template;
    }

    public Template ParseMixed(string text)
    {
        var template = Template.Parse(text, sourceFilePath: null!, DefaultLexerAndParserOptions.DefaultParserOptions, DefaultLexerAndParserOptions.MixedLexer);
        ThrowOnError(template);
        return template;
    }

    private static void ThrowOnError(Template template)
    {
        if (template.HasErrors)
        {
            throw new Exception($"Template has errors {template.Messages}");
        }
    }
}