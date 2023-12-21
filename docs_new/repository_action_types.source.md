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
| `{{ 42 < 11 \|\| true }} and 1+2 = {{ 1+2 }}` | true and 1+2 = 3 |
| `this is {{ 42 < 11 && true }}!` | this is false! |
| `Create PR ({{ repository.branch \| string.replace "feature/" "" \| string.truncate 4 ".." }})` |  Create PR abcd..  |

### Internals

Text uses the following scriban lexer and parser options:

snippet: DefaultLexerAndParserOptions_DefaultParserOptions

snippet: DefaultLexerAndParserOptions_MixedLexer

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

snippet: DefaultLexerAndParserOptions_DefaultParserOptions

snippet: DefaultLexerAndParserOptions_ScriptOnlyLexer
