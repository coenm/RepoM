# SonarCloud

This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

To enable this module, you should provide RepoM with a valid PAT by manually editing the RepoM configuration (when RepoM is not running).

## clipboard-copy@1 <!-- include: _plugins.sonarcloud.action. path: /docs/mdsource/_plugins.sonarcloud.action.include.md -->

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
