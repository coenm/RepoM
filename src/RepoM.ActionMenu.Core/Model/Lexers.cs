namespace RepoM.ActionMenu.Core.Model;

using Scriban.Parsing;

internal static class Lexers
{
    public static readonly LexerOptions ScriptOnly = new()
    {
        Lang = ScriptLang.Default, 
        Mode = ScriptMode.ScriptOnly,
    };

    public static readonly LexerOptions Mixed = new()
    {
        FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker,
        Lang = ScriptLang.Default,
        Mode = ScriptMode.Default,
    };
}