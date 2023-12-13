# WebBrowser

Provides functionality to start a web browser from an action with profile information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.WebBrowser
- PluginName: WebBrowser
- PluginDescription: Provides functionality to start a web browser from an action with profile information.
- PluginMarkdownDescription: \<empty\>

This module contains the following methods, variables and/or constants:

[## `browser@1`](#browser@1)

Action opening a webbrowser with the provided url.

Action specific properties:

- `url`: The url to browse to. ([Text](https://this-is.com/Text))
- `profile`: profile name used to select browser and browser profile ([Text](https://this-is.com/Text))
- `context`:  (Context, optional)
- `active`:  ([Predicate](https://this-is.com/Predicate))
