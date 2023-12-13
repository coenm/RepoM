# Clipboard

This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.Clipboard
- PluginName: Clipboard
- PluginDescription: Provides a 'copy to clipboard' action.
- PluginMarkdownDescription: This module provides a repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

This module contains the following methods, variables and/or constants:

[## `clipboard-copy@1`](#clipboard-copy@1)

This action makes it possible to copy text to the clipboard.

Action specific properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `text`: The text to copy to the clipboard. ([Text](https://this-is.com/Text))
- `context`:  (Context, optional)
- `active`:  ([Predicate](https://this-is.com/Predicate))
