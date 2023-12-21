# Repository Action types

## Context

sdf

## Text

Text is a scriban template for rendering textual content. These scriban templates can contain mixed content of text and scriban code blocks which are enclosed by `{{` and `}}`.

### Examples

For this example, the current repository branch name is `feature/abcdefghi`

| Input | Output (string) |
|---|---|
| `static text` | static text |
| `static {{ 1 + 2 }} text` | static 3 text |
| `{{ 42 < 11 \|\| true }}` | true |
| `{{ 42 < 11 && true }}` | false |
| `{{ 42 < 11 && true }}` | false |
| `Create PR ({{ repository.branch \| string.replace "feature/" "" \| string.strip \| string.truncate 4 ".." }})` |  Create PR abcd..  |

### Internals

Text uses the following scriban lexer and parser options:

<!-- snippet: DefaultLexerAndParserOptions_DefaultParserOptions -->
<a id='snippet-defaultlexerandparseroptions_defaultparseroptions'></a>
```cs
public static readonly ParserOptions DefaultParserOptions = new()
{
    ExpressionDepthLimit = 100,
    LiquidFunctionsToScriban = false,
    ParseFloatAsDecimal = default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L24-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultlexerandparseroptions_defaultparseroptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DefaultLexerAndParserOptions_MixedLexer -->
<a id='snippet-defaultlexerandparseroptions_mixedlexer'></a>
```cs
public static readonly LexerOptions MixedLexer = new()
{
    FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker,
    Lang = ScriptLang.Default,
    Mode = ScriptMode.Default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L15-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultlexerandparseroptions_mixedlexer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Predicate

A predicate is a scriban expression resulting in a boolean. Beause it is an expression, RepoM uses the pure scripting mode of scriban (`ScriptOnly` lexer mode) without templating (`{{` and `}}`). In other words, a Predicate is not about rendering text but evaluating a boolean expression.

### Examples

| Input | Output (boolean) |
|---|---|
| `true` | true |
| `false` | false |
| `1` | true |
| `0` | false |
| `1 == 2` | false |
| `a = [1, 2, 3]; a.size > 10` | false |
| `file.file_exists(repository.safe_path + "/readme.md")` | true or false depending if file exists |

### Internals

Predicate uses the following scriban lexer and parser options:

<!-- snippet: DefaultLexerAndParserOptions_DefaultParserOptions -->
<a id='snippet-defaultlexerandparseroptions_defaultparseroptions'></a>
```cs
public static readonly ParserOptions DefaultParserOptions = new()
{
    ExpressionDepthLimit = 100,
    LiquidFunctionsToScriban = false,
    ParseFloatAsDecimal = default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L24-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultlexerandparseroptions_defaultparseroptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DefaultLexerAndParserOptions_ScriptOnlyLexer -->
<a id='snippet-defaultlexerandparseroptions_scriptonlylexer'></a>
```cs
public static readonly LexerOptions ScriptOnlyLexer = new()
{
    Lang = ScriptLang.Default, 
    Mode = ScriptMode.ScriptOnly,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L7-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultlexerandparseroptions_scriptonlylexer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
