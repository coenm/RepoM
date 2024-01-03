# Clipboard

This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.<!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

## Configuration<!-- include: DocsModuleSettingsTests.DocsModuleSettings_ClipboardPackage#desc.verified.md -->

This module has no configuration.<!-- endInclude -->

## clipboard-copy@1<!-- include: _plugins.clipboard.action. path: /docs/mdsource/_plugins.clipboard.action.include.md -->

This action makes it possible to copy text to the clipboard.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionClipboardCopyV1.verified.md -->

Action specific properties:

- `text`: The text to copy to the clipboard. (required, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsClipboardCopy01 -->
```
** Could not find snippet 'RepositoryActionsClipboardCopy01' **
```
<!-- endSnippet -->
<!-- endInclude -->
