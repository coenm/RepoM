# Repository Action types

## Actions

Actions are a list of actions. This can be used in a sub menu.

### Example

```yaml
action-menu:
- type: folder@1
  actions:
  - type: just-text@1
    name: x
  - type: url@1
    name: x
  # etc. etc.
```

## Context

The context type enables you to create or load variables and add some custom scriban methods in order to render [Text](#text) or calculate [Predicats](#predicate).
Currenlty, RepoM supports the following types of context actions which can be added as item to the context.

- `evaluate-variable@1` A scriban template which, when evaluated, returns a value to be stored as variable.
- `evaluate-script@1` A scriban template which, when evaluated adds 'content' like variables or methods, to the current scriban context.
- `load-file@1` Loads a file (only `.env` or `.yaml`) and processes the content. An environment file is read and all environment variables are stored and accessable.
- `render-variable@1` Renders a scriban template and stores the outcomming text as variable.
- `set-variable@1` Sets a variable with static content.

## Text

Text is a scriban template for rendering textual content. These scriban templates can contain mixed content of text and scriban code blocks which are enclosed by `{{` and `}}`.

### Examples

For this example, the current repository branch name is `feature/abcdefghi` and the date is the 21st of december in 2023.

| Input | Output (string) |
|---|---|
| `static text` | static text |
| `static {{ 1 + 2 }} text` | static 3 text |
| `today is {{ date.now \| date.to_string "%Y-%m-%d" }}` | today is 2023-12-21 |
| `this is {{ 42 < 11 && true }}!` | this is false! |
| `Create PR ({{ repository.branch \| string.replace "feature/" "" \| string.truncate 4 ".." }})` |  Create PR abcd..  |

### Internals

Text uses the following scriban lexer and parser options:

<!-- snippet: DefaultLexerAndParserOptions_DefaultParserOptions -->
<a id='snippet-DefaultLexerAndParserOptions_DefaultParserOptions'></a>
```cs
public static readonly ParserOptions DefaultParserOptions = new()
{
    ExpressionDepthLimit = 100,
    LiquidFunctionsToScriban = false,
    ParseFloatAsDecimal = default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L24-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-DefaultLexerAndParserOptions_DefaultParserOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DefaultLexerAndParserOptions_MixedLexer -->
<a id='snippet-DefaultLexerAndParserOptions_MixedLexer'></a>
```cs
public static readonly LexerOptions MixedLexer = new()
{
    FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker,
    Lang = ScriptLang.Default,
    Mode = ScriptMode.Default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L15-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-DefaultLexerAndParserOptions_MixedLexer' title='Start of snippet'>anchor</a></sup>
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
| `file.file_exists(repository.path + "\readme.md")` | true or false depending if file exists |

### Internals

Predicate uses the following scriban lexer and parser options:

<!-- snippet: DefaultLexerAndParserOptions_DefaultParserOptions -->
<a id='snippet-DefaultLexerAndParserOptions_DefaultParserOptions'></a>
```cs
public static readonly ParserOptions DefaultParserOptions = new()
{
    ExpressionDepthLimit = 100,
    LiquidFunctionsToScriban = false,
    ParseFloatAsDecimal = default,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L24-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-DefaultLexerAndParserOptions_DefaultParserOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DefaultLexerAndParserOptions_ScriptOnlyLexer -->
<a id='snippet-DefaultLexerAndParserOptions_ScriptOnlyLexer'></a>
```cs
public static readonly LexerOptions ScriptOnlyLexer = new()
{
    Lang = ScriptLang.Default, 
    Mode = ScriptMode.ScriptOnly,
};
```
<sup><a href='/src/RepoM.ActionMenu.Core/Model/Lexers.cs#L7-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-DefaultLexerAndParserOptions_ScriptOnlyLexer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
