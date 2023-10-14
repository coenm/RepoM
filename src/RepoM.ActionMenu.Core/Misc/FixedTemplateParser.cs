namespace RepoM.ActionMenu.Core.Misc;

using RepoM.ActionMenu.Core.Model;
using Scriban;
using Scriban.Parsing;

internal class FixedTemplateParser : ITemplateParser
{
    private static readonly ParserOptions _parserOptions = new()
        {
            ExpressionDepthLimit = 100,
            LiquidFunctionsToScriban = false,
            ParseFloatAsDecimal = default,
        };

    public Template ParseScriptOnly(string text)
    {
        var template = Template.Parse(text, sourceFilePath: null!, _parserOptions, Lexers.ScriptOnly);
        template.ThrowOnError();
        return template;
    }

    public Template ParseMixed(string text)
    {
        var template = Template.Parse(text, sourceFilePath: null!, _parserOptions, Lexers.Mixed);
        template.ThrowOnError();
        return template;
    }
}