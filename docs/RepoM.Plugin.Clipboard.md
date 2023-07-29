# Clipboard

This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

This module has no configuration. <!-- singleLineInclude: DocsModuleSettingsTests.DocsModuleSettings_ClipboardPackage#desc.verified.md -->

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
