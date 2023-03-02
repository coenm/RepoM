# Clipboard

When this module is enabled, you can create repository actions to copy text to the clipboard using the action provider type `clipboard-action`.

## Version 1

- Type: `clipboard-copy@1`
- Properties:
  - `name`: visible name in the context menu.
  - `text`: the text to copy.
  - `active`: boolean if the action menu item is active or not.

### Example

<!-- snippet: ClipboardYaml -->
<a id='snippet-clipboardyaml'></a>
```yaml
repository-actions:
  actions:
  - type: clipboard-copy@1
    name: Copy to clipboard
    text: 'x {var.data} z'
    active: true
```
<sup><a href='/tests/RepoM.Plugin.Clipboard.Tests/ActionProvider/TestFiles/ActionClipboardCopyV1DeserializerTest.Deserialize.testfile.yaml#L1-L8' title='Snippet source file'>snippet source</a> | <a href='#snippet-clipboardyaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
