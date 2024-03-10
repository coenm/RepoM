# Clipboard

This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.Clipboard
- PluginName: Clipboard
- PluginDescription: Provides a 'copy to clipboard' action.
- PluginMarkdownDescription: This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

This module contains the following methods, variables and/or constants:

## clipboard-copy@1

This action makes it possible to copy text to the clipboard.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `text`: The text to copy to the clipboard. ([Text](docs_new/repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

### Example

<!-- snippet: clipboard-copy@1-scenario01 -->
<a id='snippet-clipboard-copy@1-scenario01'></a>
```yaml
- type: clipboard-copy@1
  name: Copy static text when feature branch
  text: 'static text'
  active: 'repository.branch | string.starts_with "feature/"'
```
<sup><a href='/tests/RepoM.Plugin.Clipboard.Tests/ActionMenu/IntegrationTests/RepositoryActionClipboardCopyV1Tests.ClipboardCopyScenario01.testfile.yaml#L3-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-clipboard-copy@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: clipboard-copy@1-scenario02 -->
<a id='snippet-clipboard-copy@1-scenario02'></a>
```yaml
- type: clipboard-copy@1
  name: Copy git checkout command to clipboard
  text: 'git checkout -b branch {{ repository.branch }}'
```
<sup><a href='/tests/RepoM.Plugin.Clipboard.Tests/ActionMenu/IntegrationTests/RepositoryActionClipboardCopyV1Tests.ClipboardCopyScenario01.testfile.yaml#L12-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-clipboard-copy@1-scenario02' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

