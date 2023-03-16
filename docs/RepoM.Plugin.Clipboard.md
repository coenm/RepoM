# Clipboard

When this module is enabled, you can create repository actions to copy text to the clipboard using the action provider type `clipboard-copy`.

## Version 1

- Type: `clipboard-copy@1`
- Properties:
  - `name`: visible name in the context menu.
  - `text`: the text to copy.
  - `active`: boolean if the action menu item is active or not.

### Example

<!-- snippet: ClipboardYaml -->
```
** Could not find snippet 'ClipboardYaml' **
```
<!-- endSnippet -->


## clipboard-copy@1 <!-- include: _plugins.clipboard.action. path: /docs/mdsource/_plugins.clipboard.action.include.md -->

This action makes it possible to copy text to the clipboard.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)
- Text: The text to copy to the clipboard (required, string, evaluated)

Example:

<!-- snippet: RepositoryActionsClipboardCopy01 -->
<a id='snippet-repositoryactionsclipboardcopy01'></a>
```yaml
repository-actions:
  actions:
  - type: clipboard-copy@1
    active: true
    variables: []
    name: Copy to clipboard
    text: ''

  - type: clipboard-copy@1
    name: Copy to clipboard
    text: ''
```
<sup><a href='/tests/RepoM.Plugin.Clipboard.Tests/DocumentationFiles/Clipboard01.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsclipboardcopy01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->
