namespace RepoM.ActionMenu.Core.Model;

using Scriban.Parsing;

internal static class DefaultLexerAndParserOptions
{
    // begin-snippet: DefaultLexerAndParserOptions_ScriptOnlyLexer
    public static readonly LexerOptions ScriptOnlyLexer = new()
    {
        Lang = ScriptLang.Default, 
        Mode = ScriptMode.ScriptOnly,
    };
    // end-snippet

    // begin-snippet: DefaultLexerAndParserOptions_MixedLexer
    public static readonly LexerOptions MixedLexer = new()
    {
        FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker,
        Lang = ScriptLang.Default,
        Mode = ScriptMode.Default,
    };
    // end-snippet

    // begin-snippet: DefaultLexerAndParserOptions_DefaultParserOptions
    public static readonly ParserOptions DefaultParserOptions = new()
    {
        ExpressionDepthLimit = 100,
        LiquidFunctionsToScriban = false,
        ParseFloatAsDecimal = default,
    };
    // end-snippet
}